using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Api;

public class Startup(IServiceCollection services)
{
    public void ConfigureServices()
    {
        ConfigureAuthentication();
        ConfigureSwagger();
    }

    private void ConfigureAuthentication()
    {
        var jwtConfig = services.BuildServiceProvider().GetService<IOptions<JwtSettings>>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true, ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Value.Secret))
            };
        });
    }

    private void ConfigureSwagger()
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("docs", new OpenApiInfo
            {
                Title = "Survey App API",
                Description = "Application programming interface"
            });
        });
    }
}
