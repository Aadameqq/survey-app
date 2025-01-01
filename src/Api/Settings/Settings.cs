namespace Api.Settings;

public static class Settings
{
    public static IServiceCollection ConfigureSettings(this IServiceCollection services)
    {
        AddSettings<AuthSettings>(services, "Auth");
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
