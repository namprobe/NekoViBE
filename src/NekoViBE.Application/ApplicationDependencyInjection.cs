using System.Reflection;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NekoViBE.Application.Common.Behaviors;
    using FluentValidation;
using NekoViBE.Application.Common.Mappings;

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
        
        // Register AutoMapper (manual, explicit profiles)
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            // Đăng ký các profile riêng (ví dụ):
            cfg.AddProfile(new AuthMappingProfile());
            cfg.AddProfile(new PaymentMethodMappingProfile());
            cfg.AddProfile(new AnimeSeriesMappingProfile());
            cfg.AddProfile(new CategoryMappingProfile());
            cfg.AddProfile(new ProductMappingProfile());
            cfg.AddProfile(new ProductInventoryMappingProfile());
            cfg.AddProfile(new ProductImageMappingProfile());
            cfg.AddProfile(new EventMappingProfile());
            cfg.AddProfile(new EventProductMappingProfile());


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