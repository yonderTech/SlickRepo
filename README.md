# SlickRepo

SlickRepo allows for quick setup of CRUD operations on an Entity Framework (Core) DbSet's.

Depending on specified generic parameters, SlickRepo will do its magic on the correct DbSet. It does the boring job of converting from a DTO to your database model 
for add and update operations and returns the resulting object as DTO back to the upper layer.

Here's how to set it up for your project.

In your Startup.cs or Program.cs

```
builder.Services.ConfigureSlickRepo(o =>
{
    o.DbIdProperty = "Id"; //The unique key property of your database objects.
    o.DtoIdProperty = "Id"; //The unique key on your DTO objects (might get rid of this as it will probably be the same on both ends in most cases)
});
```

In your web application, you can create a class that inherits SlickRepo as such.

```
public class UserModule : SlickRepo<Models.User, Dtos.User>
{
    public UserModule(YourDatabaseContext ctx, SlickRepoConfig config) : base(ctx, config)
    {

    }
}
```

You can of course create custom methods in your class by using the ``Where(Expression<Func<TDBModel, bool>> predicate)`` and ``Get(Expression<Func<TDBModel, bool>> predicate)`` methods.

You would then use ``UserModule`` in a controller like this

```
[Route("api")]
[ApiController]
public class UserController : Controller
{
  
    private UserModule Module { get; set; }
    
    public UserController(UserModule module)
    {
        Module = module;
    }

    [HttpGet]
    [Route("Users")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await Module.GetAll());
    }
}
```

That simple. Of course, you need to register ``UserModule`` as an injectable dependency

```
builder.Services.AddScoped<UserModule>();
```



Here are SlickRepo's base methods.

```
Task<List<TDto>> Where(Expression<Func<TDBModel, bool>> predicate)
Task<TDto?> Get(Expression<Func<TDBModel, bool>> predicate)
Task<TDto?> GetById(object id)
Task<List<TDto>> GetAll()
Task<TDto?> Add(TDto dto)
Task<TDto?> Update(TDto dto)
Task Delete(object id)
```
