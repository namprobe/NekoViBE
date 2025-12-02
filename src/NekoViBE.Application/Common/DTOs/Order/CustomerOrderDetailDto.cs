using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Order;

public class CustomerOrderDetailDto : CustomerOrderListItem
{
    public CustomerOrderPaymentDto? Payment { get; set; }
    public new List<CustomerOrderDetailItemDto> Items { get; set; } = new();
    
    // === COUPONS USED ===
    public List<CustomerOrderCouponDto> AppliedCoupons { get; set; } = new();
    
    // === LEGACY (for backward compatibility) ===
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? DiscountAmount { get; set; } // Deprecated: use CouponDiscountAmount + ProductDiscountAmount instead
}

public class CustomerOrderCouponDto
{
    public Guid CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountTypeEnum DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime? UsedDate { get; set; }
}

