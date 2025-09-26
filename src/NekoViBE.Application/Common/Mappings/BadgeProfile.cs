using AutoMapper;
using NekoViBE.Application.Common.DTOs.Badge;
 using NekoViBE.Application.Common.DTOs.UserBadge;
 using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.MappingProfiles;

public class BadgeProfile : Profile
{
    public BadgeProfile()
    {
        CreateMap<Badge, BadgeDto>();
        CreateMap<CreateBadgeRequest, Badge>();
        CreateMap<UpdateBadgeRequest, Badge>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IconPath, opt => opt.Ignore());

        CreateMap<Badge, UpdateBadgeRequest>()
                .ForMember(dest => dest.IconPath, opt => opt.Ignore());

        CreateMap<UserBadge, UserBadgeDto>();
        CreateMap<AssignBadgeToUserRequest, UserBadge>();
    }
}