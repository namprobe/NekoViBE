using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

public class StaffProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public GenderEnum? Gender { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public string? LeaveReason { get; set; }
    public decimal? Salary { get; set; }
    public StaffPositionEnum? Position { get; set; }
    // navigation property
    public virtual AppUser? User { get; set; }
}