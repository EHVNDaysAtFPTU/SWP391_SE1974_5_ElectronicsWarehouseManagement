using ElectronicsWarehouseManagement.WebAPI.Services;

public static class Ext
{
    public static IServiceCollection AddDIService(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}