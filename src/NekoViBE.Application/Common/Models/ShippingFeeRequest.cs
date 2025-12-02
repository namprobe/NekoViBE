namespace NekoViBE.Application.Common.Models;

public class ShippingFeeRequest
{
    public int FromDistrictId { get; set; }
    public string FromWardCode { get; set; } = string.Empty;
    public int ToDistrictId { get; set; }
    public string ToWardCode { get; set; } = string.Empty;
    public int ServiceTypeId { get; set; } = 2; // 2: Hàng nhẹ, 5: Hàng nặng
    public int Weight { get; set; } // grams
    public int Length { get; set; } // cm
    public int Width { get; set; } // cm
    public int Height { get; set; } // cm
    public int? InsuranceValue { get; set; }
    public int? CodAmount { get; set; }
    public string? Coupon { get; set; }
}

