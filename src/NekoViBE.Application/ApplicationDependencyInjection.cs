using System.Reflection;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NekoViBE.Application.Common.Behaviors;
    using FluentValidation;
using NekoViBE.Application.Common.Mappings;
using NekoViBE.Application.Features.Payment.Services;

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

        // Register AutoMapper with DI support (allows constructor injection in resolvers)
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register Payment services
        services.AddScoped<ICallBackShareLogic, CallBackShareLogic>();
        
        return services;
    }
}