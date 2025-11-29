namespace NekoViBE.Application.Common.Models;

public class ShippingLeadTimeResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public ShippingLeadTimeData? Data { get; set; }
}

public class ShippingLeadTimeData
{
    public DateTime? LeadTime { get; set; }
    public long? LeadTimeUnix { get; set; }
    public long? OrderDateUnix { get; set; }
    public DateTime? EstimateFrom { get; set; }
    public DateTime? EstimateTo { get; set; }
}

