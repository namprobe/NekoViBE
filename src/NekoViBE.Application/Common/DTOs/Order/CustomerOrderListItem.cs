using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Order;

/// <summary>
/// Lightweight order summary tailored for customer portal consumption.
/// Hides CMS-only identity fields while emphasizing order content.
/// </summary>
public class CustomerOrderListItem : BaseResponse
{
    public bool IsOneClick { get; set; }
    
    // === BREAKDOWN PHÍ ===
    public decimal SubtotalOriginal { get; set; }
    public decimal ProductDiscountAmount { get; set; }
    public decimal SubtotalAfterProductDiscount { get; set; }
    public decimal CouponDiscountAmount { get; set; }
    public decimal TotalProductAmount { get; set; }
    public decimal ShippingFeeOriginal { get; set; }
    public decimal ShippingDiscountAmount { get; set; }
    public decimal ShippingFeeActual { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalAmount { get; set; }
    
    // === STATUS ===
    public PaymentStatusEnum PaymentStatus { get; set; }
    public OrderStatusEnum OrderStatus { get; set; }
    
    // === CONTENT ===
    public List<CustomerOrderItemDTO> Items { get; set; } = new();
    public CustomerOrderShippingDto? Shipping { get; set; }
    
    // === LEGACY (for backward compatibility) ===
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? TotalAmount { get; set; } // Deprecated: use SubtotalOriginal instead
}

