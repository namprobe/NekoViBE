using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrderId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Title { get; set; }
    public string? Comment { get; set; }
    
    // navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
    public virtual Order? Order { get; set; } = null!;
}
