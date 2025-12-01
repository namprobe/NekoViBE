using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

public class UserAddress : BaseEntity
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public AddressTypeEnum AddressType { get; set; } = AddressTypeEnum.Home;
    public string Address { get; set; } = string.Empty; // Địa chỉ chi tiết (số nhà, tên đường)
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; } = true;
    public string? PhoneNumber { get; set; }
    
    // GHN identifiers captured directly from client
    public int? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public int? DistrictId { get; set; }
    public string? DistrictName { get; set; }
    public string? WardCode { get; set; }
    public string? WardName { get; set; }
    
    // Legacy fields (deprecated, keep for backward compatibility during migration)
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    
    // navigation properties
    public virtual AppUser? User { get; set; }
}