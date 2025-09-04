using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class PostTag : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid TagId { get; set; }
    
    // navigation properties
    public virtual BlogPost Post { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}
