using AutoMapper;
using NekoViBE.Application.Common.DTOs.Auth;
using NekoViBE.Application.Features.Auth.Queries.GetProfile;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<RegisterRequest, AppUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EntityStatusEnum.Active))
            .ForMember(dest => dest.JoiningAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<RegisterRequest, CustomerProfile>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => EntityStatusEnum.Active));

        CreateMap<AppUser, ProfileResponse>()
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.CustomerProfile != null ? src.CustomerProfile.Gender.ToString() :
            src.StaffProfile != null ? src.StaffProfile.Gender.ToString() : null))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.CustomerProfile != null ? src.CustomerProfile.DateOfBirth : null))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.StaffProfile != null ? src.StaffProfile.Position.ToString() : null))
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.StaffProfile != null ? src.StaffProfile.HireDate : null))
            .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.StaffProfile != null ? src.StaffProfile.Salary : null));
    }
}