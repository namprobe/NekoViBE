using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.UserAddress;

public class UserAddressRequest
{
    public AddressTypeEnum AddressType { get; set; } = AddressTypeEnum.Home;
    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Vietnam";
    public bool IsDefault { get; set; } = true;
    public string? PhoneNumber { get; set; }
    public EntityStatusEnum Status { get; set; } = EntityStatusEnum.Active;
}