using System;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Order;

/// <summary>
/// Coupon information for CMS order detail view.
/// Shows which coupons were applied to the order.
/// </summary>
public class OrderCouponDto
{
    public Guid CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountTypeEnum DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime? UsedDate { get; set; }
}

