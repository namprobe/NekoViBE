namespace NekoViBE.Application.Common.DTOs.UserAddress;

public class UserAddressItem : BaseResponse
{
    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public int? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public int? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? WardCode { get; set; }
    public string? WardName { get; set; }
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; } = true;
    public string? PhoneNumber { get; set; }
}