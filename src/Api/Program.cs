using System.Text;
using Api;
using Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JwtConfig>()
    .BindConfiguration("Jwt")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<DatabaseConfig>()
    .BindConfiguration("Database")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var jwtConfig = builder.Services.BuildServiceProvider().GetService<IOptions<JwtConfig>>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true, ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Value.Secret))
    };
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("docs", new OpenApiInfo
    {
        Title = "Survey App API",
        Description = "Application programming interface"
    });
});

builder.Services.AddDbContext<DatabaseContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => { c.RouteTemplate = "api-docs/{documentName}/swagger.json"; });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api-docs/docs/swagger.json", "Docs");
        c.RoutePrefix = "api-docs";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
