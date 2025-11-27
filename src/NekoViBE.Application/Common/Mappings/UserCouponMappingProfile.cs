using AutoMapper;
using NekoViBE.Application.Common.DTOs.UserCoupon;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings;

public class UserCouponMappingProfile : Profile
{
    public UserCouponMappingProfile()
    {
        CreateMap<UserCoupon, UserCouponItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.CouponId, opt => opt.MapFrom(src => src.CouponId))
            .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.Coupon.Code))
            .ForMember(dest => dest.CouponName, opt => opt.MapFrom(src => src.Coupon.Code))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Coupon.Description))
            .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src => src.Coupon.DiscountType))
            .ForMember(dest => dest.DiscountValue, opt => opt.MapFrom(src => src.Coupon.DiscountValue))
            .ForMember(dest => dest.MinOrderAmount, opt => opt.MapFrom(src => src.Coupon.MinOrderAmount))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Coupon.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Coupon.EndDate))
            .ForMember(dest => dest.UsageLimit, opt => opt.MapFrom(src => src.Coupon.UsageLimit))
            .ForMember(dest => dest.CurrentUsage, opt => opt.MapFrom(src => src.Coupon.CurrentUsage));

        CreateMap<UserCoupon, UserCouponDetail>()
            .IncludeBase<UserCoupon, UserCouponItem>();
    }
}

