using AutoMapper;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Mappings
{
    public class AnimeSeriesMappingProfile : Profile
    {
        public AnimeSeriesMappingProfile()
        {
            CreateMap<AnimeSeriesRequest, AnimeSeries>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

            CreateMap<AnimeSeries, AnimeSeriesItem>();
            CreateMap<AnimeSeries, AnimeSeriesResponse>();

            // Ánh xạ từ AnimeSeries sang AnimeSeriesRequest
            CreateMap<AnimeSeries, AnimeSeriesRequest>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ReleaseYear, opt => opt.MapFrom(src => src.ReleaseYear ?? 0))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        }
    }
}
