using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class BlogPost : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? AuthorId { get; set; }
    public Guid? PostCategoryId { get; set; }
    public DateTime PublishDate { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; } = true;
    public string? FeaturedImagePath { get; set; }
    
    // navigation properties
    public virtual AppUser? Author { get; set; }
    public virtual PostCategory? PostCategory { get; set; }
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
