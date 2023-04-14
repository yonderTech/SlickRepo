using SlickRepo;
using Microsoft.OpenApi.Models;
using System;
using WebTest;
using WebTest.Module;
using Dto = WebTest.Dtos;
using Model = WebTest.DB.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


builder.Services.AddDbContext<WebTestContext>(ServiceLifetime.Scoped);

builder.Services.AddSlickRepo<Model.IBaseModelGuid<Model.BaseModelGuid>, Dto.IBaseDtoGuid<Dto.BaseDtoGuid>>(o =>
{
    o.DbIdProperty = "Id";
    o.DtoIdProperty = "Id";

}, ServiceLifetime.Scoped);

builder.Services.AddScoped<UserModule>();


builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.ToString());
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SlickRepo WebTest", Version = "v1" });
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebTest v1"));

app.Run();


