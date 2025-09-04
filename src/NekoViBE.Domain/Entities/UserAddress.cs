using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

public class UserAddress : BaseEntity
{
    public Guid UserId { get; set; }
    public AddressTypeEnum AddressType { get; set; } = AddressTypeEnum.Home;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = true;
    public string? PhoneNumber { get; set; }
    // navigation property
    public virtual AppUser? User { get; set; }
}