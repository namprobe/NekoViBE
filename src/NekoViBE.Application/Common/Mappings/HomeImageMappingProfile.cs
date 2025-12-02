using AutoMapper;
using NekoViBE.Application.Common.DTOs.HomeImage;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings
{
    public class HomeImageMappingProfile : Profile
    {
        public HomeImageMappingProfile()
        {
            CreateMap<HomeImage, HomeImageItem>()
                .ForMember(dest => dest.AnimeSeriesName,
                    opt => opt.MapFrom(src => src.AnimeSeries != null ? src.AnimeSeries.Title : null));

            CreateMap<HomeImage, HomeImageResponse>()
                .ForMember(dest => dest.AnimeSeriesName,
                    opt => opt.MapFrom(src => src.AnimeSeries != null ? src.AnimeSeries.Title : null));
        }
    }
}