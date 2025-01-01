using Api.Infrastructure;
using Api.Models;
using Api.Models.Interfaces;

namespace Api;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureDependencies(this IServiceCollection services)
    {
        services.AddDbContext<DatabaseContext>();
        services.AddScoped<PasswordHasher, BCryptPasswordService>();
        services.AddScoped<PasswordVerifier, BCryptPasswordService>();
        services.AddScoped<UsersRepository, EfUsersRepository>();
        services.AddScoped<RefreshTokensRepository, EfRefreshTokensRepository>();
        services.AddScoped<AuthSessionsRepository, EfAuthSessionsRepository>();
        services.AddScoped<RefreshTokensFactory, RefreshTokensFactoryImpl>();
        services.AddScoped<TokenService, AspTokenService>();
        return services;
    }
}
