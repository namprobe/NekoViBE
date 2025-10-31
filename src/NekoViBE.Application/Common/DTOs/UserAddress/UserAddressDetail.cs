using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.UserAddress;

public class UserAddressDetail : BaseResponse
{
    public string FullName { get; set; } = string.Empty;
    public AddressTypeEnum AddressType { get; set; }
    public string AddressTypeEnumName => AddressType.ToString();
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = true;
    public string? PhoneNumber { get; set; }
}