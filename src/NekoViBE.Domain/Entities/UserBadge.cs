using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class UserBadge : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid BadgeId { get; set; }
    public DateTime EarnedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? ActivatedFrom { get; set; }
    public DateTime? ActivatedTo { get; set; }
    
    // navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual Badge Badge { get; set; } = null!;
}
