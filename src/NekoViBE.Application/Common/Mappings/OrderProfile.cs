using AutoMapper;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Mappings
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

            CreateMap<Order, OrderListItem>()
               .ForMember(dest => dest.UserEmail,
                   opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
               .ForMember(dest => dest.UserName,
                   opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
               .ForMember(dest => dest.GuestName,
                   opt => opt.MapFrom(src => src.GuestFirstName != null && src.GuestLastName != null
                       ? $"{src.GuestFirstName} {src.GuestLastName}"
                       : null))
               .ForMember(dest => dest.ItemCount,
                   opt => opt.MapFrom(src => src.OrderItems.Sum(oi => oi.Quantity)));


            CreateMap<OrderItem, OrderItemDetailDTO>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductImage,
                    opt => opt.MapFrom(src => src.Product.ProductImages
                        .FirstOrDefault(pi => pi.IsPrimary) != null
                        ? src.Product.ProductImages.First(pi => pi.IsPrimary).ImagePath
                        : src.Product.ProductImages.FirstOrDefault() != null
                            ? src.Product.ProductImages.First().ImagePath
                            : null));
        }
    }
}
