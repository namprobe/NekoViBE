using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NekoViBE.Infrastructure.Context;

namespace NekoViBE.API.Extensions;

public static class OuterDbMigrationExtension
{
    /// <summary>
    /// Apply pending migrations for Outer Database (External services, Hangfire)
    /// </summary>
    public static async Task ApplyOuterDbMigrationsAsync(this IApplicationBuilder app, ILogger logger)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var outerDbContext = scope.ServiceProvider.GetService<NekoViOuterDbContext>();
        
        if (outerDbContext == null)
        {
            logger.LogWarning("NekoViOuterDbContext is not registered. Skipping outer database migrations.");
            return;
        }

        try
        {
            var pendingMigrations = await outerDbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations to Outer Database...", pendingMigrations.Count());
                await outerDbContext.Database.MigrateAsync();
                logger.LogInformation("Outer Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("No pending migrations for Outer Database.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying Outer Database migrations");
            throw;
        }
    }
}
