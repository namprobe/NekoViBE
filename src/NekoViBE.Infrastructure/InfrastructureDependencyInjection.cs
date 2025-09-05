using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Infrastructure.Context;
using NekoViBE.Infrastructure.Repositories;
using NekoViBE.Infrastructure.Services;

namespace NekoViBE.Infrastructure;

public static class InfrastructureDependencyInjection
{

    /// <summary>
    /// Add infrastructure services to the dependency container
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Please se configure it in appsettings.json");
        }

        // Configure database contexts
        services.AddDbContextPool<NekoViDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(NekoViDbContext).Assembly.FullName);
                sql.CommandTimeout(30);
            });
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(CoreEventId.FirstWithoutOrderByAndFilterWarning));
        });
        // Register contexts as interfaces
        services.AddScoped<INekoViDbContext>(provider => provider.GetRequiredService<NekoViDbContext>());
        // Map DbContext for services that depend on base DbContext (e.g., UnitOfWork)
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<NekoViDbContext>());

        // Configure Identity
        services.AddIdentity<AppUser, AppRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;

            // User settings
            options.User.RequireUniqueEmail = true;  // Changed to true for email verification

            // SignIn settings  
            options.SignIn.RequireConfirmedEmail = true;   // Changed to true for email verification
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<NekoViDbContext>()
        .AddDefaultTokenProviders();

        // Configure email token lifespan (24 hours)
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Configure Storage settings
        //services.Configure<StorageSettings>(configuration.GetSection("Storage"));

        // Configure VNPay settings
        //services.Configure<VNPaySettings>(configuration.GetSection("VNPay"));

        // Configure Email settings
        //services.Configure<EmailSettings>(configuration.GetSection("Email"));

        // Register repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}