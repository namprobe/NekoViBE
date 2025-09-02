using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NekoViBE.Infrastructure.Context;

namespace NekoViBE.API.Extensions;

public static class MigrationExtension
{
    /// <summary>
    /// Tự động apply các migration pending cho NekoViBE database
    /// </summary>
    /// <param name="app">IApplicationBuilder</param>
    /// <param name="logger">ILogger</param>
    /// <returns>Task</returns>
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var nekoViContext = scope.ServiceProvider.GetRequiredService<NekoViDbContext>();

            logger.LogInformation("Starting NekoVi database migrations...");

            // Kiểm tra kết nối database với retry logic
            try
            {
                await RetryDatabaseConnectionAsync(nekoViContext, "NekoVi", logger);
                logger.LogInformation("Successfully connected to NekoVi database.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to NekoVi database after multiple attempts!");
                throw;
            }

            // Apply pending migrations cho NekoVi
            try
            {
                var pendingMigrations = await nekoViContext.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await nekoViContext.Database.GetAppliedMigrationsAsync();

                logger.LogInformation(
                    "NekoVi DB: Found {PendingCount} pending migrations and {AppliedCount} previously applied migrations",
                    pendingMigrations.Count(),
                    appliedMigrations.Count());

                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying pending NekoVi migrations: {Migrations}",
                        string.Join(", ", pendingMigrations));

                    await nekoViContext.Database.MigrateAsync();
                    logger.LogInformation("Successfully applied all pending NekoVi migrations.");
                }
                else
                {
                    logger.LogInformation("No pending migrations found for NekoVi database.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying NekoVi migrations!");
                throw;
            }

            logger.LogInformation("NekoVi database migrations completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A problem occurred during NekoVi database migrations!");
            throw;
        }
    }

    /// <summary>
    /// Thử kết nối database với retry logic
    /// </summary>
    private static async Task RetryDatabaseConnectionAsync(DbContext context, string contextName,
        ILogger logger, int maxRetries = 3, int delaySeconds = 5)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Attempting to connect to {ContextName} database (attempt {Attempt}/{MaxRetries})...",
                    contextName, attempt, maxRetries);

                // Test connection
                await context.Database.CanConnectAsync();
                logger.LogInformation("Successfully connected to {ContextName} database on attempt {Attempt}",
                    contextName, attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex,
                    "Failed to connect to {ContextName} database on attempt {Attempt}. Retrying in {DelaySeconds} seconds...",
                    contextName, attempt, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to {ContextName} database after {MaxRetries} attempts",
                    contextName, maxRetries);
                throw;
            }
        }
    }

    /// <summary>
    /// Đảm bảo database được tạo nếu chưa tồn tại
    /// </summary>
    public static void EnsureDatabaseCreated(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var nekoViDbContext = scope.ServiceProvider.GetRequiredService<NekoViDbContext>();

            logger.LogInformation("Checking if NekoVi database exists...");

            if (nekoViDbContext.Database.EnsureCreated())
            {
                logger.LogInformation("NekoVi database was created successfully.");
            }
            else
            {
                logger.LogInformation("NekoVi database already exists.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while ensuring NekoVi database exists!");
            throw;
        }
    }
}

