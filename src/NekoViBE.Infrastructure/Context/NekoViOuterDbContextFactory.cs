using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NekoViBE.Infrastructure.Context;

public class NekoViOuterDbContextFactory : IDesignTimeDbContextFactory<NekoViOuterDbContext>
{
    public NekoViOuterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NekoViOuterDbContext>();
        
        // Đọc connection string từ appsettings.json của WebApp
        var webAppPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "NekoViBE.API");
        
        // Lấy environment từ biến môi trường (Azure tự set ASPNETCORE_ENVIRONMENT=Production)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(webAppPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("OuterDbConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'OuterDbConnection' not found. Make sure appsettings.json exists in NekoViBE.API project.");
        }

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.MigrationsAssembly(typeof(NekoViOuterDbContext).Assembly.FullName);
            });

        return new NekoViOuterDbContext(optionsBuilder.Options);
    }
}
