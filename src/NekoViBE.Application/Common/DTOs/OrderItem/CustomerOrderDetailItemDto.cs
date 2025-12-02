using System;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.OrderItem;

public class CustomerOrderDetailItemDto : CustomerOrderItemDTO
{
    public string CategoryName { get; set; } = string.Empty;
    public string? AnimeSeriesName { get; set; }
    public bool IsPreOrder { get; set; }
    public DateTime? PreOrderReleaseDate { get; set; }
}

