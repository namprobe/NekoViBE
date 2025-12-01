using AutoMapper;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Mappings
{
    public class CouponMappingProfile : Profile
    {
        public CouponMappingProfile()
        {
            CreateMap<Coupon, CouponDto>()
                .ForMember(dest => dest.CurrentUsage, opt => opt.MapFrom(src => src.CurrentUsage));
            
            CreateMap<CreateCouponRequest, Coupon>()
                .ForMember(dest => dest.CurrentUsage, opt => opt.Ignore()); // Initialize to 0 by default
            
            CreateMap<UpdateCouponRequest, Coupon>()
                .ForMember(dest => dest.CurrentUsage, opt => opt.Ignore()); // Never update CurrentUsage from request
            
            CreateMap<Coupon, CouponItem>()
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.Status == EntityStatusEnum.Active))
                .ForMember(dest => dest.CurrentUsage, opt => opt.MapFrom(src => src.CurrentUsage));
        }
    }
}
