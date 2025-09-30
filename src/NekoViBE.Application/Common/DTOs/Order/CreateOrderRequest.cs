using NekoViBE.Application.Common.DTOs.OrderItem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Order
{
    public class CreateOrderRequest
    {
        [Required]
        public List<OrderItemRequest> OrderItems { get; set; } = new();

        public string? CouponCode { get; set; }
        //public Guid ShippingAddressId { get; set; }
        //public Guid ShippingMethodId { get; set; }
        //public Guid PaymentMethodId { get; set; }
        public string? Notes { get; set; }
        public bool IsOneClick { get; set; } = false;

        // For guest checkout
        public string? GuestEmail { get; set; }
        public string? GuestFirstName { get; set; }
        public string? GuestLastName { get; set; }
        public string? GuestPhone { get; set; }
        public string? OneClickAddress { get; set; }
    }
}
