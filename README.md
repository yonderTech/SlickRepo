# SlickRepo

SlickRepo allows for quick setup of CRUD operations on an Entity Framework (Core) DbSet's.

Depending on specified generic parameters, SlickRepo will do its magic on the correct DbSet. It does the boring job of converting from a DTO to your database model 
for add and update operations and returns the resulting object as DTO back to the upper layer.

Here's how to set it up for your project.

In your Startup.cs or Program.cs

```
builder.Services.ConfigureSlickRepo(o =>
{
    o.DbIdProperty = "Id";
    o.DtoIdProperty = "Id";

});
```
