namespace NekoViBE.Application.Common.Models;

public class ShippingLeadTimeRequest
{
    public int FromDistrictId { get; set; }
    public string FromWardCode { get; set; } = string.Empty;
    public int ToDistrictId { get; set; }
    public string ToWardCode { get; set; } = string.Empty;
    public int ServiceTypeId { get; set; } = 2;
    public int? ServiceId { get; set; }
}

