using NekoViBE.API.Injection;
using NekoViBE.Application;
using NekoViBE.Infrastructure;

namespace NekoViBE.API.Configurations;

public static class ServiceConfiguration
{
    /// <summary>
    /// Register all application services and infrastructure to keep Program.cs minimal
    /// </summary>
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        // Core ASP.NET services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Custom Swagger configuration with tagging and styling
        builder.Services.AddSwaggerConfiguration();

        // Cross-cutting concerns
        builder.AddLoggingConfiguration();
        
        // Security configurations
        builder.Services.AddJwtConfiguration(builder.Configuration);
        builder.Services.AddCorsConfiguration(builder.Configuration);

        // Application & Infrastructure layers
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        // API layer services (filters, middlewares, validation, etc.)
        builder.Services.AddApiServices(builder.Configuration);

        return builder;
    }

    /// <summary>
    /// Configure the middleware pipeline
    /// </summary>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Use custom Swagger configuration with styling and tagging
            app.UseSwaggerConfiguration(app.Environment);
        }

        app.UseHttpsRedirection();
        
        // Enable static files for Swagger custom CSS
        app.UseStaticFiles();
        
        // Enable CORS
        app.UseCorsConfiguration();

        // API-specific middlewares (exception handling, JWT, etc.)
        app.UseApiConfiguration();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
