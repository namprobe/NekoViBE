using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class AnimeSeries : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public int? ReleaseYear { get; set; }
    
    // navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}