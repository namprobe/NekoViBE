namespace NekoViBE.Application.Common.Models;

public class ShippingCallbackResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public ShippingCallbackData? Data { get; set; }
}

public class ShippingCallbackData
{
    public string OrderCode { get; set; } = string.Empty;
    public string? ClientOrderCode { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
}

