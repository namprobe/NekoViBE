using System;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.UserCoupon;

public class UserCouponItem : BaseResponse
{
    public Guid CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string? CouponName { get; set; }
    public string? Description { get; set; }
    public DiscountTypeEnum DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int CurrentUsage { get; set; }
    public DateTime? UsedDate { get; set; }
    public bool IsUsed => UsedDate.HasValue;
    public bool IsExpired => EndDate < DateTime.UtcNow;
    public string DiscountTypeName => DiscountType.ToString();
}

