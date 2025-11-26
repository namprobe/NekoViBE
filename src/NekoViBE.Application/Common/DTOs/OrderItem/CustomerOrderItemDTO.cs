using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.OrderItem
{
    public class CustomerOrderItemDTO : BaseResponse
    {
        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; } 

        public string ProductName { get; set; } = string.Empty;

        public string? ProductImage { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal ItemTotal => (UnitPrice * Quantity) - DiscountAmount;
    }
}
