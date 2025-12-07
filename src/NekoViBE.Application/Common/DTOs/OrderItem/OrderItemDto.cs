using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.OrderItem
{
    /// <summary>
    /// Order item DTO for CMS view.
    /// Contains detailed pricing breakdown matching the OrderItem entity.
    /// </summary>
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        
        // === PRICING BREAKDOWN ===
        /// <summary>
        /// Original price per unit (Product.Price)
        /// </summary>
        public decimal UnitPriceOriginal { get; set; }
        
        /// <summary>
        /// Price after product discount per unit
        /// </summary>
        public decimal UnitPriceAfterDiscount { get; set; }
        
        /// <summary>
        /// Discount amount per unit
        /// </summary>
        public decimal UnitDiscountAmount { get; set; }
        
        /// <summary>
        /// Total for this line (UnitPriceAfterDiscount × Quantity)
        /// </summary>
        public decimal LineTotal { get; set; }
    }
}
