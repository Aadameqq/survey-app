using Api.Models;

namespace Api;

// namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureDependencies(this IServiceCollection services)
    {
        services.AddDbContext<DatabaseContext>();
        return services;
    }
}
