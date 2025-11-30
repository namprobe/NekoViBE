using AutoMapper;
using NekoViBE.Application.Common.DTOs.Shipping;

namespace NekoViBE.Application.Common.Mappings;

public class ShippingHistoryProfile : Profile
{
    public ShippingHistoryProfile()
    {
        CreateMap<Domain.Entities.ShippingHistory, ShippingHistoryDto>();
    }
}

