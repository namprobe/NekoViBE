namespace NekoViBE.Application.Common.Models;

public class ShippingPreviewResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public ShippingPreviewData? Data { get; set; }
}

public class ShippingPreviewData
{
    public string OrderCode { get; set; } = string.Empty;
    public ShippingOrderFee? Fee { get; set; }
    public int TotalFee { get; set; }
    public DateTime? ExpectedDeliveryTime { get; set; }
}

