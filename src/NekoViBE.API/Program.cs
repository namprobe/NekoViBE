using NekoViBE.API.Configurations;
using NekoViBE.API.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .ConfigureServices();

var app = builder.Build()
    .ConfigurePipeline();

if (app.Environment.IsDevelopment())
{
    // Apply pending migrations automatically on startup
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigration");
    await app.ApplyMigrationsAsync(logger);
    
    // Seed initial data (roles, admin) if enabled
    await app.SeedInitialDataAsync(logger);
}

app.Run();
