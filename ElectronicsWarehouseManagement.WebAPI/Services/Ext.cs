using ElectronicsWarehouseManagement.WebAPI.Services;

public static class Ext
{
    public static IServiceCollection AddDIService(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IStorekeeperService, StorekeeperService>();
<<<<<<< HEAD
=======
        services.AddScoped<IManagerService, ManagerService>();
>>>>>>> main
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}