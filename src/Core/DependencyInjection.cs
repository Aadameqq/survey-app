using Core.Application;
using Core.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureCoreDependencies(this IServiceCollection services)
    {
        services.AddDbContext<PostgresContext>();
        services.AddScoped<GreetingInteractor>();

        return services;
    }
}
