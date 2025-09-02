using Microsoft.AspNetCore.Identity;
using NekoViBE.Domain.Enums;
using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class AppRole : IdentityRole<Guid>, IEntityLike
{
    public string Description { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public EntityStatusEnum Status { get; set; }
    // navigation property
    public virtual ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}