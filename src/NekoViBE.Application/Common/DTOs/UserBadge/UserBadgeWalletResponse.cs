namespace NekoViBE.Application.Common.DTOs.UserBadge
{
    public class UserBadgeWalletResponse
    {
        public List<UserBadgeWalletItem> Unlocked { get; set; } = new();
        public List<BadgeProgressItem> Locked { get; set; } = new();
    }
}
