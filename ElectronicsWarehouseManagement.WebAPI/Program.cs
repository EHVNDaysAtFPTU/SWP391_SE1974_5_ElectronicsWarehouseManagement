using ElectronicsWarehouseManagement.Repositories.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ElectronicsWarehouseManagement.WebAPI
{
    public class Program
    {
        static TimeSpan sessionTimeout = TimeSpan.FromMinutes(10);

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<EWMDbCtx>(options => options.UseSqlServer(connectionString));

            builder.Services.AddControllers();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = sessionTimeout;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;

                    options.ExpireTimeSpan = sessionTimeout;
                    options.SlidingExpiration = true;

                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToAccessDenied = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        },
                        OnRedirectToLogin = async context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json; charset=utf-8";
                            ApiResult payload = new ApiResult(ApiResultCode.Unauthorized);
                            //if (!string.IsNullOrWhiteSpace(context.HttpContext.Session.GetString("User")))
                                //payload = new ApiResult(ApiResultCode.SessionExpired);
                            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                        }
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDIService();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseSession();

            app.UseAuthentication(); 
            app.UseAuthorization();

            //app.UseDefaultFiles();

            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    OnPrepareResponse = ctx =>
            //    {
            //        if (!ctx.Context.Request.Path.StartsWithSegments("/view", StringComparison.OrdinalIgnoreCase))
            //            return;
            //        ctx.Context.Response.StatusCode = StatusCodes.Status404NotFound;
            //        ctx.Context.Response.ContentLength = 0;
            //        ctx.Context.Response.Body = Stream.Null;
            //    }
            //});

            app.MapControllers();

            app.Run();
        }
    }
}
