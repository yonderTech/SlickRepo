using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

namespace SlickRepo
{
    /// <summary>
    /// BaseModule, provides base operations for CRUD operations and converting back and forth
    /// </summary>
    /// <typeparam name="TModel">DB model type</typeparam>
    /// <typeparam name="TDto">Dto model type</typeparam>
    public class SlickRepo<TModel, TDto> where TModel : class where TDto : class
    {
        public SlickRepoConfig<TModel, TDto> Config { get; set; }
        public DbContext Context { get; set; }

        public SlickRepo(DbContext context, SlickRepoConfig<TModel, TDto> config)
        {
            Context = context;
            Config = config;
        }

        /// <summary>
        /// Returns a list matching provided predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<List<TDto>> Where(Expression<Func<TModel, bool>> predicate)
        {
            var data = await DbSet.Where(predicate).ToListAsync();
            return ConvertToDto(data);
        }

        /// <summary>
        /// Returns a single record, based on provided predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<TDto?> Get(Expression<Func<TModel, bool>> predicate)
        {
            var exists = await DbSet.SingleOrDefaultAsync(predicate);
            var errorMsg = $"{ModuleName}.Get({predicate}): record not found";

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
        public async Task<TDto?> GetById(object id)
        {
            var exists = await DbSet.SingleOrDefaultAsync(x => Config.DbIdProperty.Invoke(x) == id);

            var errorMsg = $"{ModuleName}.GetById({id}): record not found";

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
        public async Task<List<TDto>> GetAll()
        {
            var data = await DbSet.ToListAsync();
            return ConvertToDto(data);
        }

        /// <summary>
        /// Add to DbSet
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<TDto?> Add(TDto dto)
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
                var errorMsg = $"{ModuleName}.Add({JsonConvert.SerializeObject(dto)}): error adding record: {ex.Message}";
                throw new Exception(errorMsg);
            }
        }

        /// <summary>
        /// Update a specified record
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<TDto?> Update(TDto dto)
        {
            try
            {
                var target = await DbSet.SingleOrDefaultAsync(x => Config.DbIdProperty.Invoke(x) == Config.DtoIdProperty.Invoke(dto));
                ApplyProperties(dto, target);
                await Context.SaveChangesAsync();
                return ConvertToDto(target);
            }
            catch (Exception ex)
            {
                var errorMsg = $"{ModuleName}.Update({JsonConvert.SerializeObject(dto)}): error updating record: {ex.Message}";
                throw new Exception(errorMsg);
            }


        }

        /// <summary>
        /// Delete a record from DbSet, with provided Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task Delete(object id)
        {
            var dbModel = await DbSet.SingleOrDefaultAsync(m => Config.DbIdProperty.Invoke(m) == id);
            if (dbModel != null)
            {
                DbSet.Remove(dbModel);
                await Context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Search through context for a DbSet<> with generic constraint matching TModel
        /// </summary>
        private DbSet<TModel>? DbSet
        {
            get
            {
                var dbSetProperty = Context.GetType().GetProperties().SingleOrDefault(x => x.PropertyType == typeof(DbSet<TModel>));
                if (dbSetProperty == null)
                    throw new Exception($"{ModuleName}.DbSet: No DbSet<{typeof(TModel).Name}> found in context!");

                var o = dbSetProperty.GetValue(Context);

                if (o == null)
                    throw new Exception($"{ModuleName}.DbSet: DbSet<{typeof(TModel).Name}> is null value.");

                DbSet<TModel>? dbSet = o as DbSet<TModel>;
                return dbSet;

            }
        }

        private string ModuleName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// Converts a record from TModel to TDto
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private TDto? ConvertToDto(TModel item)
        {
            return ConvertLogic<TDto>(item);
        }

        private TModel? ConvertToModel(TDto dto)
        {
            return ConvertLogic<TModel>(dto);
        }

        /// <summary>
        /// Converts a list of record of type TModel to a list of records of type TDto
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private List<TDto> ConvertToDto(List<TModel> items)
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
            var serializedModel = JsonConvert.SerializeObject(model, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore

            });

            if (serializedModel != null)
                return JsonConvert.DeserializeObject<T>(serializedModel);
            else
                return default(T);
        }


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

    }
}
