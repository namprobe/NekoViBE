using Microsoft.AspNetCore.Identity;

namespace NekoViBE.Domain.Entities;

public class AppUserRole : IdentityUserRole<Guid>
{
    public virtual AppUser User { get; set; } = default!;
    public virtual AppRole Role { get; set; } = default!;
}
