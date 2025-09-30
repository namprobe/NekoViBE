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
            CreateMap<Coupon, CouponDto>();
            CreateMap<CreateCouponRequest, Coupon>();
            CreateMap<UpdateCouponRequest, Coupon>();
            CreateMap<Coupon, CouponItem>()
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.Status == EntityStatusEnum.Active));
        }
    }
}
