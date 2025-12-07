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
            
            // === CMS MAPPINGS ===
            
            // OrderItem -> OrderItemDto (for CMS) - convert image path to full URL
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, 
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductImageUrl,
                    opt => opt.ConvertUsing<FilePathUrlConverter, string?>(src => ExtractProductImagePath(src)));

            // Order -> OrderListItem (for CMS table view)
            CreateMap<Order, OrderListItem>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.UtcNow))
                .ForMember(dest => dest.UserEmail,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : null))
                .ForMember(dest => dest.GuestName,
                    opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.GuestFirstName) || !string.IsNullOrWhiteSpace(src.GuestLastName)
                        ? $"{src.GuestFirstName} {src.GuestLastName}".Trim()
                        : null))
                .ForMember(dest => dest.ItemCount,
                    opt => opt.MapFrom(src => src.OrderItems.Count));

            // OrderShippingMethod -> OrderShippingDto (for CMS)
            CreateMap<OrderShippingMethod, OrderShippingDto>()
                .ForMember(dest => dest.ShippingMethodName,
                    opt => opt.MapFrom(src => src.ShippingMethod != null ? src.ShippingMethod.Name : null))
                .ForMember(dest => dest.RecipientName,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.FullName : null))
                .ForMember(dest => dest.RecipientPhone,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.PhoneNumber : null))
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.Address : null))
                .ForMember(dest => dest.WardName,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.WardName : null))
                .ForMember(dest => dest.DistrictName,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.DistrictName : null))
                .ForMember(dest => dest.ProvinceName,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.ProvinceName : null));

            // Payment -> OrderPaymentDto (for CMS)
            CreateMap<Payment, OrderPaymentDto>()
                .ForMember(dest => dest.PaymentMethodName,
                    opt => opt.MapFrom(src => src.PaymentMethod != null ? src.PaymentMethod.Name : string.Empty));

            // UserCoupon -> OrderCouponDto (for CMS)
            CreateMap<Domain.Entities.UserCoupon, OrderCouponDto>()
                .ForMember(dest => dest.CouponCode,
                    opt => opt.MapFrom(src => src.Coupon != null ? src.Coupon.Code : string.Empty))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.Coupon != null ? src.Coupon.Description : null))
                .ForMember(dest => dest.DiscountType,
                    opt => opt.MapFrom(src => src.Coupon != null ? src.Coupon.DiscountType : default))
                .ForMember(dest => dest.DiscountValue,
                    opt => opt.MapFrom(src => src.Coupon != null ? src.Coupon.DiscountValue : 0));

            // Order -> OrderDto (for CMS detail view, extends OrderListItem)
            CreateMap<Order, OrderDto>()
                .IncludeBase<Order, OrderListItem>()
                .ForMember(dest => dest.OrderItems,
                    opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.Shipping,
                    opt => opt.MapFrom(src => src.OrderShippingMethods.FirstOrDefault()))
                .ForMember(dest => dest.Payment,
                    opt => opt.MapFrom(src => src.Payment))
                .ForMember(dest => dest.AppliedCoupons,
                    opt => opt.MapFrom(src => src.UserCoupons.Where(uc => uc.Coupon != null)));

            // === CUSTOMER MAPPINGS ===


            CreateMap<OrderItem, CustomerOrderItemDTO>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductImage,
                    opt => opt.ConvertUsing<FilePathUrlConverter, string?>(src => ExtractProductImagePath(src)))
                .ForMember(dest => dest.UnitPrice,
                    opt => opt.MapFrom(src => src.UnitPriceAfterDiscount))
                .ForMember(dest => dest.DiscountAmount,
                    opt => opt.MapFrom(src => src.UnitDiscountAmount * src.Quantity));

            // OrderShippingMethod -> CustomerOrderShippingDto (for customer app)
            CreateMap<OrderShippingMethod, CustomerOrderShippingDto>()
                .ForMember(dest => dest.ShippingMethodName,
                    opt => opt.MapFrom(src => src.ShippingMethod != null ? src.ShippingMethod.Name : null))
                .ForMember(dest => dest.RecipientName,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.FullName : null))
                .ForMember(dest => dest.RecipientPhone,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.PhoneNumber : null))
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.Address : null))
                .ForMember(dest => dest.WardName,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.WardName : null))
                .ForMember(dest => dest.DistrictName,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.DistrictName : null))
                .ForMember(dest => dest.ProvinceName,
                    opt => opt.MapFrom(src => src.UserAddress != null ? src.UserAddress.ProvinceName : null))
                .ForMember(dest => dest.ShippingStatus,
                    opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderStatus : default));

            CreateMap<Order, CustomerOrderListItem>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.Shipping,
                    opt => opt.MapFrom(src => src.OrderShippingMethods.FirstOrDefault()))
                .ForMember(dest => dest.SubtotalOriginal,
                    opt => opt.MapFrom(src => src.SubtotalOriginal))
                .ForMember(dest => dest.ProductDiscountAmount,
                    opt => opt.MapFrom(src => src.ProductDiscountAmount))
                .ForMember(dest => dest.SubtotalAfterProductDiscount,
                    opt => opt.MapFrom(src => src.SubtotalAfterProductDiscount))
                .ForMember(dest => dest.CouponDiscountAmount,
                    opt => opt.MapFrom(src => src.CouponDiscountAmount))
                .ForMember(dest => dest.TotalProductAmount,
                    opt => opt.MapFrom(src => src.TotalProductAmount))
                .ForMember(dest => dest.ShippingFeeOriginal,
                    opt => opt.MapFrom(src => src.ShippingFeeOriginal))
                .ForMember(dest => dest.ShippingDiscountAmount,
                    opt => opt.MapFrom(src => src.ShippingDiscountAmount))
                .ForMember(dest => dest.ShippingFeeActual,
                    opt => opt.MapFrom(src => src.ShippingFeeActual))
                .ForMember(dest => dest.TaxAmount,
                    opt => opt.MapFrom(src => src.TaxAmount))
                .ForMember(dest => dest.TotalAmount,
                    opt => opt.MapFrom(src => src.SubtotalOriginal)); // Legacy: map to SubtotalOriginal

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

            CreateMap<Domain.Entities.UserCoupon, CustomerOrderCouponDto>()
                .ForMember(dest => dest.CouponId, opt => opt.MapFrom(src => src.CouponId))
                .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.Coupon.Code))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Coupon.Description))
                .ForMember(dest => dest.DiscountType, opt => opt.MapFrom(src => src.Coupon.DiscountType))
                .ForMember(dest => dest.DiscountValue, opt => opt.MapFrom(src => src.Coupon.DiscountValue))
                .ForMember(dest => dest.UsedDate, opt => opt.MapFrom(src => src.UsedDate));

            CreateMap<Order, CustomerOrderDetailDto>()
                .IncludeBase<Order, CustomerOrderListItem>()
                .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment))
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.AppliedCoupons,
                    opt => opt.MapFrom(src => src.UserCoupons.Where(uc => uc.UsedDate != null && uc.Coupon != null)))
                .ForMember(dest => dest.DiscountAmount,
                    opt => opt.MapFrom(src => src.CouponDiscountAmount + src.ProductDiscountAmount)); // Legacy: sum of all discounts
        }
    }
}
