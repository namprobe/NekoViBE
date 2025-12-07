using System;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Order;

/// <summary>
/// Shipping information for CMS order detail view.
/// Contains detailed shipping method and tracking information.
/// </summary>
public class OrderShippingDto
{
    public Guid Id { get; set; }
    public Guid ShippingMethodId { get; set; }
    public string? ShippingMethodName { get; set; }
    public string? ProviderName { get; set; }
    public string? TrackingNumber { get; set; }
    
    // === SHIPPING ADDRESS ===
    public Guid? UserAddressId { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }
    public string? Address { get; set; }
    public string? WardName { get; set; }
    public string? DistrictName { get; set; }
    public string? ProvinceName { get; set; }
    
    // === SHIPPING FEES ===
    public decimal ShippingFeeOriginal { get; set; }
    public decimal ShippingDiscountAmount { get; set; }
    public decimal ShippingFeeActual { get; set; }
    public bool IsFreeshipping { get; set; }
    public string? FreeshippingNote { get; set; }
    
    // === ADDITIONAL FEES ===
    public decimal CodFee { get; set; }
    public decimal InsuranceFee { get; set; }
    
    // === DATES ===
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
}

