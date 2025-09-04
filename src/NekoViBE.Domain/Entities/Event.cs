using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class Event : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? ImagePath { get; set; }
    
    // navigation properties
    public virtual ICollection<EventProduct> EventProducts { get; set; } = new List<EventProduct>();
}
