using AutoMapper;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Mappings
{
    public class EventMappingProfile : Profile
    {
        public EventMappingProfile()
        {
            CreateMap<EventRequest, Event>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ImagePath, opt => opt.Ignore());

            CreateMap<Event, EventItem>();
            CreateMap<Event, EventResponse>();

            CreateMap<Event, EventRequest>()
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore());
        }
    }
}
