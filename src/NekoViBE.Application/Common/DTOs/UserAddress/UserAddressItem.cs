namespace NekoViBE.Application.Common.DTOs.UserAddress;

public class UserAddressItem : BaseResponse
{
    public string FullName { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = true;
    public string? PhoneNumber { get; set; }
}