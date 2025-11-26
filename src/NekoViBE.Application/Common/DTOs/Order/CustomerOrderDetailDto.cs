using System.Collections.Generic;
using System.Text.Json.Serialization;
using NekoViBE.Application.Common.DTOs.OrderItem;

namespace NekoViBE.Application.Common.DTOs.Order;

public class CustomerOrderDetailDto : CustomerOrderListItem
{
    public decimal DiscountAmount { get; set; }
    public CustomerOrderPaymentDto? Payment { get; set; }
    public new List<CustomerOrderDetailItemDto> Items { get; set; } = new();
}

