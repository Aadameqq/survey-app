using System.Text;
using Api;
using Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSettings();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var jwtConfig = builder.Configuration.GetRequiredSection("Auth").Get<AuthSettings>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidateIssuer = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig!.AccessTokenSecret)),
        ValidateAudience = false, //TODO: is this needed
        ValidIssuer = jwtConfig.Issuer
    };
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Wprowadź token JWT w formacie: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    options.SwaggerDoc("docs", new OpenApiInfo
    {
        Title = "Survey App API",
        Description = "Application programming interface"
    });
});

builder.Services.ConfigureDependencies();

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
