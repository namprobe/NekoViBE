using NekoViBE.Infrastructure.Configurations;
using NekoViBE.Infrastructure.Filters;

namespace NekoViBE.API.Extensions;

/// <summary>
/// Extension methods for application startup configuration
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Configure application - ĐÃ DỌN DẸP, CHỈ CÒN DATABASE VÀ SERVICE ACCOUNT TEST
    /// </summary>
    public static async Task<WebApplication> ConfigureApplicationAsync(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ApplicationStartup");
        
        try
        {
            // Step 1: Apply main database migrations
            logger.LogInformation("Applying main database migrations...");
            await app.ApplyMigrationsAsync(logger);
            
            // Step 2: Apply outer database migrations
            logger.LogInformation("Applying outer database migrations...");
            await app.ApplyOuterDbMigrationsAsync(logger);
            
            logger.LogInformation("Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations.");
            throw;
        }
        
        if (app.Environment.IsDevelopment())
        {
            // Step 3: Seed initial data (only in development)
            logger.LogInformation("Seeding initial data...");
            await app.SeedInitialDataAsync(logger);
        }
        
        logger.LogInformation("Application configuration completed successfully");
        return app;
    }
}
