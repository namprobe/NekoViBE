using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class UserHomeImage : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid HomeImageId { get; set; }
    public int Position { get; set; } // 1, 2 hoặc 3 → thứ tự hiển thị

    // Navigation
    public virtual AppUser User { get; set; } = null!;
    public virtual HomeImage HomeImage { get; set; } = null!;
}