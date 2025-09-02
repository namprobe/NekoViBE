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
        builder.Services.AddSwaggerGen();

        // Cross-cutting concerns
        builder.AddLoggingConfiguration();

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
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // API-specific middlewares (exception handling, JWT, etc.)
        app.UseApiConfiguration();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
