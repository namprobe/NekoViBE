using Microsoft.Extensions.DependencyInjection;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Domain.Enums;
using NekoViBE.Infrastructure.Services;

namespace NekoViBE.Infrastructure.Factories;

public class ShippingServiceFactory : IShippingServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ShippingServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IShippingService GetShippingService(ShippingProviderType providerType)
    {
        return providerType switch
        {
            ShippingProviderType.GHN => _serviceProvider.GetRequiredService<GHNService>(),
            // FUTURE: Add other shipping providers here
            // ShippingProviderType.GHTK => _serviceProvider.GetRequiredService<GHTKService>(),
            // ShippingProviderType.ViettelPost => _serviceProvider.GetRequiredService<ViettelPostService>(),
            // ShippingProviderType.JNT => _serviceProvider.GetRequiredService<JNTService>(),
            _ => throw new ArgumentException($"Invalid shipping provider type: {providerType}"),
        };
    }
}

