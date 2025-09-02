using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

public class CustomerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public GenderEnum? Gender { get; set; }
    // navigation property
    public virtual AppUser? User { get; set; }

}