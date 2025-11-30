namespace NekoViBE.Application.Common.Models;

public class ShippingOrderResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public ShippingOrderData? Data { get; set; }
}

public class ShippingOrderData
{
    public string OrderCode { get; set; } = string.Empty;
    public string SortCode { get; set; } = string.Empty;
    public string TransType { get; set; } = string.Empty;
    public ShippingOrderFee? Fee { get; set; }
    public int TotalFee { get; set; }
    public DateTime? ExpectedDeliveryTime { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}

public class ShippingOrderFee
{
    public int MainService { get; set; }
    public int Insurance { get; set; }
    public int StationDo { get; set; }
    public int StationPu { get; set; }
    public int Return { get; set; }
    public int R2S { get; set; }
    public int Coupon { get; set; }
    public int CodFailedFee { get; set; }
}

