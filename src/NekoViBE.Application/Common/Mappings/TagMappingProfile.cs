using AutoMapper;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings
{
    public class TagMappingProfile : Profile
    {
        public TagMappingProfile()
        {
            CreateMap<TagRequest, Tag>()
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Tag, TagRequest>();
            CreateMap<Tag, TagItem>();
            CreateMap<Tag, TagResponse>();
        }
    }
}
