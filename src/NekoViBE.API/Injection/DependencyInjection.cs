using NekoViBE.API.Attributes;
using NekoViBE.API.Middlewares;

namespace NekoViBE.API.Injection;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add HttpContextAccessor
        services.AddHttpContextAccessor();
        
        // Register role-based access filters
        services.AddScoped<CustomerRoleAccessFilter>();
        services.AddScoped<StaffRoleAccessFilter>();
        services.AddScoped<AdminRoleAccessFilter>();

        // Register validation configuration
        services.AddValidationConfiguration();


        return services;
    }

    public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app)
    {
        // Use global exception handling
        app.UseGlobalExceptionHandling();

        // Use JWT middleware
        app.UseJwtMiddleware();

        return app;
    }
} 