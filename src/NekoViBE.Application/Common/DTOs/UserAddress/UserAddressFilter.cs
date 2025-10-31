using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.UserAddress;

public class UserAddressFilter : BasePaginationFilter
{
    public Guid? UserId { get; set; }
    public bool? IsDefault { get; set; }
    public AddressTypeEnum? AddressType { get; set; }
    public bool? IsCurrentUser { get; set; } = true; // If true, the user address will be filtered by the current user
}