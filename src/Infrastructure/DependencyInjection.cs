using Core.Ports;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Infrastructure.smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureInfrastructureDependencies(
        this IServiceCollection services
    )
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
        });

        services.AddDbContext<DatabaseContext>();
        services.AddScoped<PasswordHasher, BCryptPasswordService>();
        services.AddScoped<PasswordVerifier, BCryptPasswordService>();
        services.AddScoped<AccountsRepository, EfAccountsRepository>();
        services.AddScoped<RefreshTokensRepository, EfRefreshTokensRepository>();
        services.AddScoped<AuthSessionsRepository, EfAuthSessionsRepository>();
        services.AddScoped<RefreshTokensFactory, RefreshTokensFactoryImpl>();
        services.AddScoped<ActivationEmailBodyGenerator, ActivationEmailBodyGeneratorImpl>();
        services.AddScoped<EmailSender, SystemEmailSender>();
        services.AddScoped<ActivationCodesRepository, RedisActivationCodesRepository>();
        services.AddSingleton<AccessTokenService, AspAccessTokenService>();
        return services;
    }
}
