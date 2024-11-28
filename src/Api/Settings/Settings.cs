namespace Api.Settings;

public static class Settings
{
    public static IServiceCollection ConfigureSettings(this IServiceCollection services)
    {
        AddSettings<JwtSettings>(services, "Jwt");
        AddSettings<DatabaseSettings>(services, "Database");
        return services;
    }

    private static void AddSettings<T>(IServiceCollection services, string sectionName) where T : class
    {
        services.AddOptions<T>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
