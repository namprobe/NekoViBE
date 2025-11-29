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
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; }
    public OrderStatusEnum OrderStatus { get; set; }
    public List<CustomerOrderItemDTO> Items { get; set; } = new();
    public CustomerOrderShippingDto? Shipping { get; set; }
}

