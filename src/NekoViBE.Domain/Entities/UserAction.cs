using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities
{
    public class UserAction : BaseEntity
    {
        public Guid UserId { get; set; }
        public UserActionEnum Action { get; set; }
        public Guid? EntityId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IPAddress { get; set; }
        public string? ActionDetail { get; set; }
        // navigation property
        public virtual AppUser? User { get; set; }
    }
}