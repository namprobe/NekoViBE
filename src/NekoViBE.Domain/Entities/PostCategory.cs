using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class PostCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // navigation properties
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
