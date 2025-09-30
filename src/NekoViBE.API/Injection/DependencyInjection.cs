using MediatR;
using NekoViBE.API.Attributes;
using NekoViBE.API.Middlewares;
using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Order.OrderBusinessLogic;
using NekoViBE.Application.Features.OrderItem.Query.GetOrderItemsByOrderId;
using NekoViBE.Infrastructure.Factories;
using NekoViBE.Infrastructure.Services;

namespace NekoViBE.API.Injection;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add HttpContextAccessor
        services.AddHttpContextAccessor();

        // Đăng ký FileServiceFactory
        services.AddSingleton<IFileServiceFactory, FileServiceFactory>();
        services.AddScoped<ICreateOrderService, CreateOrderService>();
        //services.AddScoped<IRequestHandler<GetOrderItemsByOrderIdQuery, Result<List<OrderItemDetailDTO>>>, GetOrderItemsByOrderIdQueryHandler>();


        // Đăng ký FileService (sử dụng factory để lấy instance cụ thể)
        services.AddScoped<IFileService>(provider =>
        {
            var factory = provider.GetRequiredService<IFileServiceFactory>();
            var storageType = configuration["FileStorage:Type"] ?? "local";
            return factory.CreateFileService(storageType);
        });

        // Register role-based access filters
        services.AddScoped<CustomerRoleAccessFilter>();
        services.AddScoped<StaffRoleAccessFilter>();
        services.AddScoped<AdminRoleAccessFilter>();

        // Register validation configuration
        services.AddValidationConfiguration();


        return services;
    }

    public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app)
    {
        // Use global exception handling
        app.UseGlobalExceptionHandling();

        // Use JWT middleware
        app.UseJwtMiddleware();

        return app;
    }
} 