using System;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Order;

public class CustomerOrderShippingDto
{
    public string? ShippingMethodName { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public OrderStatusEnum? ShippingStatus { get; set; }
}


