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

You would then use ``UserModule`` in a controller

