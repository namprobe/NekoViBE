// Domain/Entities/HomeImage.cs
using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class HomeImage : BaseEntity
{
    public string ImagePath { get; set; } = string.Empty;

    // FK tới AnimeSeries (có thể null nếu là ảnh chung)
    public Guid? AnimeSeriesId { get; set; }

    // Navigation
    public virtual AnimeSeries? AnimeSeries { get; set; }
    public virtual ICollection<UserHomeImage> UserSelections { get; set; } = new List<UserHomeImage>();
}