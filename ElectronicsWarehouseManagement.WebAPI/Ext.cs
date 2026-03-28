using ElectronicsWarehouseManagement.WebAPI.Services;

namespace ElectronicsWarehouseManagement.WebAPI;

public static class Ext
{
    public static IServiceCollection AddDIService(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>(); 
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IStorekeeperService, StorekeeperService>();
        services.AddScoped<IManagerService, ManagerService>();
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}