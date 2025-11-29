// Application/Common/Mappings/UserHomeImageMappingProfile.cs
using AutoMapper;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings
{
    public class UserHomeImageMappingProfile : Profile
    {
        public UserHomeImageMappingProfile()
        {
            CreateMap<UserHomeImageRequest, UserHomeImage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<UserHomeImage, UserHomeImageItem>()
                .ForMember(dest => dest.HomeImage, opt => opt.MapFrom(src => src.HomeImage));

            CreateMap<UserHomeImage, UserHomeImageResponse>()
                .IncludeBase<UserHomeImage, UserHomeImageItem>();

            CreateMap<UserHomeImageSaveRequest, UserHomeImage>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.UserId, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
        }
    }
}