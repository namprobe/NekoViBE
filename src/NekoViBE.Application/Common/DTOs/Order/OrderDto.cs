using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Order
{
    /// <summary>
    /// Detailed order information for CMS.
    /// Extends OrderListItem with additional details for full order view.
    /// </summary>
    public class OrderDto : OrderListItem
    {
        // === GUEST DETAILS (additional fields not in list) ===
        public string? GuestFirstName { get; set; }
        public string? GuestLastName { get; set; }
        public string? GuestPhone { get; set; }
        public string? OneClickAddress { get; set; }
        
        // === ORDER DETAILS ===
        public string? Notes { get; set; }
        
        // === ITEMS ===
        public List<OrderItemDto> OrderItems { get; set; } = new();
        
        // === SHIPPING ===
        public OrderShippingDto? Shipping { get; set; }
        
        // === PAYMENT ===
        public OrderPaymentDto? Payment { get; set; }
        
        // === COUPONS ===
        public List<OrderCouponDto> AppliedCoupons { get; set; } = new();
    }
}
