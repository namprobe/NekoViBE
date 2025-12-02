using MediatR;

namespace NekoViBE.Application.Features.UserBadge.Command.SyncBadgeCoupons
{
    /// <summary>
    /// Command to sync badge coupons for users who have badges
    /// This ensures all badge holders have their linked coupons in UserCoupons table
    /// </summary>
    public record SyncBadgeCouponsCommand : IRequest<Common.Models.Result>
    {
        /// <summary>
        /// Optional: Sync for specific user only. If null, syncs for all users
        /// </summary>
        public Guid? UserId { get; init; }
    }
}
