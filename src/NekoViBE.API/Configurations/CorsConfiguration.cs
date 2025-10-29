namespace NekoViBE.API.Configurations;

/// <summary>
/// Configuration for Cross-Origin Resource Sharing (CORS)
/// </summary>
public static class CorsConfiguration
{
    private const string ProductionCorsPolicy = "ProductionCorsPolicy";
    private const string DevelopmentCorsPolicy = "DevelopmentCorsPolicy";
    
    /// <summary>
    /// Configure CORS with environment-specific policies
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Extract allowed origins from configuration
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            // Production-specific policy with strict security
            options.AddPolicy(ProductionCorsPolicy, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowCredentials(); // Allow cookies and auth headers
                }
                else
                {
                    // Fallback: không có origins thì allow any (không khuyến khích)
                    policy.AllowAnyOrigin();
                }
                
                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition", "Token-Expired", "X-Pagination");
            });

            // Development-specific policy with looser security
            options.AddPolicy(DevelopmentCorsPolicy, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowCredentials(); // Allow cookies and auth headers
                }
                else
                {
                    policy.AllowAnyOrigin();
                }
                
                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition", "Token-Expired", "X-Pagination");
            });
        });
        
        return services;
    }

    /// <summary>
    /// Use the environment-appropriate CORS policy
    /// </summary>
    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        // Get current environment
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        
        // Use the appropriate policy based on environment
        if (environment.IsProduction())
        {
            app.UseCors(ProductionCorsPolicy);
        }
        else
        {
            app.UseCors(DevelopmentCorsPolicy);
        }
        
        return app;
    }
}