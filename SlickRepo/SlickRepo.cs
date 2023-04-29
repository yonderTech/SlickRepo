using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace SlickRepo
{
    /// <summary>
    /// SlickRepo. Provides base operations for CRUD operations and converting back and forth to database and upper layer(s)
    /// </summary>
    /// <typeparam name="TDBModel">DB model type</typeparam>
    /// <typeparam name="TDto">Dto model type</typeparam>
    public abstract class SlickRepo<TDBModel, TDto> where TDBModel : class where TDto : class
    {
        public DbContext Context { get; set; }
        private string DbIdPropertyName { get; set; }
        private string DtoIdPropertyName { get; set; }
        private JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

    private DbSet<TDBModel>? DbSet { get; set; }

        /// <summary>
        /// SlickRepo.
        /// </summary>
        /// <param name="context">Your DbContext</param>
        /// <param name="dbIdPropertyExpression">Expression to specify TDBModel's unique key</param>
        /// <param name="dtoIdPropertyExpression">Expression to specify TDto's unique key. Optional, will assume same as TDBModel if nothing provided</param>
        /// <exception cref="Exception"></exception>
        public SlickRepo(DbContext context, 
            Expression<Func<TDBModel, object>> dbIdPropertyExpression,
            Expression<Func<TDto, object>> dtoIdPropertyExpression = null)
        {
            Context = context;
            
            DbIdPropertyName = PropertyName(dbIdPropertyExpression);

            if (dtoIdPropertyExpression == null)
                DtoIdPropertyName = DbIdPropertyName;
            else
                DtoIdPropertyName = PropertyName(dtoIdPropertyExpression);

            var dbSetProperty = Context.GetType().GetProperties().SingleOrDefault(x => x.PropertyType == typeof(DbSet<TDBModel>));

            if (dbSetProperty == null)
                throw new Exception($"SlickRepo.ctor: No DbSet<{typeof(TDBModel).Name}> found in context!");

            var o = dbSetProperty.GetValue(Context);

            if (o == null)
                throw new Exception($"SlickRepo.ctor: DbSet<{typeof(TDBModel).Name}> is null value.");

           DbSet = o as DbSet<TDBModel>;
        }

        /// <summary>
        /// Returns a list matching provided predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual async Task<List<TDto>> Where(Expression<Func<TDBModel, bool>> predicate)
        {
            var data = await DbSet.Where(predicate).ToListAsync();
            return ConvertToDto(data);
        }

        /// <summary>
        /// Returns a single record, based on provided predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual async Task<TDto?> Get(Expression<Func<TDBModel, bool>> predicate)
        {
            var exists = await DbSet.SingleOrDefaultAsync(predicate);
            var errorMsg = $"{ClassName}.Get({predicate}): record not found";

            if (exists == null)
            {
                throw new Exception(errorMsg);
            }

            return ConvertToDto(exists);
        }

        /// <summary>
        /// Get record, by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual TDto? GetById(object id)
        {
            var exists = DbSet.AsEnumerable().SingleOrDefault(ById(id));
            var errorMsg = $"{ClassName}.GetById({id}): record not found";

            if (exists == null)
            {
                throw new Exception(errorMsg);
            }

            return ConvertToDto(exists);
        }

        /// <summary>
        /// Gets the entire DbSet as List<>
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<TDto>> GetAll()
        {
            var data = await DbSet.ToListAsync();
            return ConvertToDto(data);
        }

        /// <summary>
        /// Add to DbSet
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual async Task<TDto?> Add(TDto dto)
        {

            try
            {
                var dbModel = ConvertToModel(dto);
                await DbSet.AddAsync(dbModel);
                await Context.SaveChangesAsync();
                return ConvertToDto(dbModel);
            }
            catch (Exception ex)
            {
                var errorMsg = $"{ClassName}.Add({JsonConvert.SerializeObject(dto)}): error adding record: {ex.Message}";
                throw new Exception(errorMsg);
            }
        }

        /// <summary>
        /// Update a specified record
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual async Task<TDto?> Update(TDto dto)
        {
            try
            {
                var dtoId = dto.GetType().GetProperty(DtoIdPropertyName).GetValue(dto);
                var target = DbSet.AsEnumerable().SingleOrDefault(ById(dtoId));
                ApplyProperties(dto, target);
                await Context.SaveChangesAsync();
                return ConvertToDto(target);
            }
            catch (Exception ex)
            {
                var errorMsg = $"{ClassName}.Update({JsonConvert.SerializeObject(dto)}): error updating record: {ex.Message}";
                throw new Exception(errorMsg);
            }


        }

        /// <summary>
        /// Delete a record from DbSet, with provided Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task Delete(object id)
        {
            var dbModel = DbSet.AsEnumerable().SingleOrDefault(ById(id));
            if (dbModel != null)
            {
                DbSet.Remove(dbModel);
                await Context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Converts a record from TModel to TDto
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public TDto? ConvertToDto(TDBModel item)
        {
            return ConvertLogic<TDto>(item);
        }

        public TDBModel? ConvertToModel(TDto dto)
        {
            return ConvertLogic<TDBModel>(dto);
        }

        private string ClassName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// Converts a list of record of type TModel to a list of records of type TDto
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private List<TDto> ConvertToDto(List<TDBModel> items)
        {
            List<TDto> results = new List<TDto>();
            foreach (var item in items)
            {
                var convertedItem = ConvertLogic<TDto>(item);
                if (convertedItem != null)
                    results.Add(convertedItem);
            }
            return results;
        }

        /// <summary>
        /// Converts a record from TModel to TDto
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private T? ConvertLogic<T>(object model)
        {
            var serializedModel = JsonConvert.SerializeObject(model, Formatting.None, JsonSerializerSettings);

            if (!string.IsNullOrEmpty(serializedModel))
                return JsonConvert.DeserializeObject<T>(serializedModel);
            else
                throw new Exception($"{ClassName}.ConvertLogic(): Error serializing input object.");
        }

        /// <summary>
        /// Apply properties from an object to another
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private void ApplyProperties(object source, object target)
        {
            List<PropertyInfo> targetProperties = target.GetType().GetProperties().ToList();
            List<PropertyInfo> sourceProperties = source.GetType().GetProperties().ToList();


            foreach (var sourceProperty in sourceProperties)
            {
                var targetProperty = targetProperties.SingleOrDefault(pi => pi.Name.ToLower() == sourceProperty.Name.ToLower());
                if (targetProperty != null)
                    targetProperty.SetValue(target, sourceProperty.GetValue(source));

            }
        }


        /// <summary>
        /// Return a Func filtering by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private Func<TDBModel, bool> ById(object id)
        {
            var prop = typeof(TDBModel).GetProperty(DbIdPropertyName);
            if (prop == null)
                throw new Exception($"{ClassName}.ById({id}): Error retrieving property info '{DbIdPropertyName}' on provided TDBModel.");

            return x => x != null && prop.GetValue(x) != null && prop.GetValue(x)?.ToString() == id.ToString();
        }


        /// <summary>
        /// Return the string value for a property provided through a lambda expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private string PropertyName<T>(Expression<Func<T, object>> propertyExpression)
        {
            MemberExpression mbody = propertyExpression.Body as MemberExpression;

            if (mbody == null)
            {
                //This will handle Nullable<T> properties.
                UnaryExpression ubody = propertyExpression.Body as UnaryExpression;

                if (ubody != null)
                {
                    mbody = ubody.Operand as MemberExpression;
                }

                if (mbody == null)
                {
                    throw new ArgumentException("Expression is not a MemberExpression", "propertyExpression");
                }
            }

            return mbody.Member.Name;
        }

    }
}
