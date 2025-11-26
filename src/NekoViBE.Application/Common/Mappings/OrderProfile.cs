using System.Linq;
using AutoMapper;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Application.Common.Mappings.Resolvers;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.Mappings
{
    public class OrderProfile : Profile
    {
        private static string? ExtractProductImagePath(OrderItem orderItem)
        {
            if (orderItem.Product == null || orderItem.Product.ProductImages == null || !orderItem.Product.ProductImages.Any())
                return null;

            var primaryImage = orderItem.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary);
            if (primaryImage != null)
                return primaryImage.ImagePath;

            var firstImage = orderItem.Product.ProductImages.FirstOrDefault();
            return firstImage != null ? firstImage.ImagePath : null;
        }

        public OrderProfile()
        {
            CreateMap<PlaceOrderRequest, Order>()
                .IgnoreBaseEntityFields();
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


            CreateMap<OrderItem, CustomerOrderItemDTO>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductImage,
                    opt => opt.ConvertUsing<FilePathUrlConverter, string?>(src => ExtractProductImagePath(src)));

            CreateMap<Order, CustomerOrderListItem>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, CustomerOrderDetailItemDto>()
                .IncludeBase<OrderItem, CustomerOrderItemDTO>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Product.Category != null ? src.Product.Category.Name : string.Empty))
                .ForMember(dest => dest.AnimeSeriesName,
                    opt => opt.MapFrom(src => src.Product.AnimeSeries != null ? src.Product.AnimeSeries.Title : null))
                .ForMember(dest => dest.IsPreOrder,
                    opt => opt.MapFrom(src => src.Product.IsPreOrder))
                .ForMember(dest => dest.PreOrderReleaseDate,
                    opt => opt.MapFrom(src => src.Product.PreOrderReleaseDate));

            CreateMap<Payment, CustomerOrderPaymentDto>()
                .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PaymentMethodId, opt => opt.MapFrom(src => src.PaymentMethodId))
                .ForMember(dest => dest.PaymentMethodName,
                    opt => opt.MapFrom(src => src.PaymentMethod.Name));

            CreateMap<Order, CustomerOrderDetailDto>()
                .IncludeBase<Order, CustomerOrderListItem>()
                .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment))
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.OrderItems));
        }
    }
}
