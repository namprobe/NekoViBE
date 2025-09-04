using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // navigation properties
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
