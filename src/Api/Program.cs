using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("default", new OpenApiInfo
    {
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
    });
});

builder.Services.ConfigureCoreDependencies();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api-docs/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api-docs/default/swagger.json", "Default");
        c.RoutePrefix = "api-docs";
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();