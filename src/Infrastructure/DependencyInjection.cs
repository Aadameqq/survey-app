using Core.Ports;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructureDependencies(
        this IServiceCollection services
    )
    {
        services.AddDbContext<DatabaseContext>();
        services.AddScoped<PasswordHasher, BCryptPasswordService>();
        services.AddScoped<PasswordVerifier, BCryptPasswordService>();
        services.AddScoped<UsersRepository, EfUsersRepository>();
        services.AddScoped<RefreshTokensRepository, EfRefreshTokensRepository>();
        services.AddScoped<AuthSessionsRepository, EfAuthSessionsRepository>();
        services.AddScoped<RefreshTokensFactory, RefreshTokensFactoryImpl>();
        services.AddSingleton<AccessTokenService, AspAccessTokenService>();
        return services;
    }
}
