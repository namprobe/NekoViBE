using AutoMapper;
using NekoViBE.Application.Common.DTOs.Role;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Features.User.Commands.CreateUser;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<AppUser, UserDTO>();
            CreateMap<AppRole, RoleInfoDTO>();

            // Mapping from command to entity
            CreateMap<CreateUserCommand, AppUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.AvatarPath, opt => opt.MapFrom(src => src.AvatarPath))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForAllMembers(opts => opts.Ignore());

        }
    }
}
