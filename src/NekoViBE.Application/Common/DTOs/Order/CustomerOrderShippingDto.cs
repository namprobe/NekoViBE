using System;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Order;

public class CustomerOrderShippingDto
{
    public string? ShippingMethodName { get; set; }
    public string? TrackingNumber { get; set; }
    
    // === SHIPPING ADDRESS ===
    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }
    public string? Address { get; set; }
    public string? WardName { get; set; }
    public string? DistrictName { get; set; }
    public string? ProvinceName { get; set; }
    
    // === DATES & STATUS ===
    public DateTime? ShippedDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public OrderStatusEnum? ShippingStatus { get; set; }
}


