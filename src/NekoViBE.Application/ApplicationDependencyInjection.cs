using System.Reflection;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NekoViBE.Application.Common.Behaviors;
    using FluentValidation;

namespace NekoViBE.Application;

public static class ApplicationDependencyInjection
{
    /// <summary>
    /// Add application services to the dependency container
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Add pipeline behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        });
        
        // Register AutoMapper (manual, explicit profiles like GameHub)
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            // Đăng ký các profile riêng (ví dụ):
            // cfg.AddProfile(new UserMappingProfile());
            // cfg.AddProfile(new ProductMappingProfile());

            // Và tự động quét tất cả Profile trong assembly này    
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });
        IMapper mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
        
        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}