using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.UserAddress;

public class UserAddressRequest
{
    public AddressTypeEnum AddressType { get; set; } = AddressTypeEnum.Home;
    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ProvinceId { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string WardCode { get; set; } = string.Empty;
    public string WardName { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; } = true;
    public string? PhoneNumber { get; set; }
    public EntityStatusEnum Status { get; set; } = EntityStatusEnum.Active;
}