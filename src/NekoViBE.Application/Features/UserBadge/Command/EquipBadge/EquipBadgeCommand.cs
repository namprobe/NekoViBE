using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserBadge.Command.EquipBadge
{
    public class EquipBadgeCommand : IRequest<Result>
    {
        public Guid BadgeId { get; set; }

        public EquipBadgeCommand(Guid badgeId)
        {
            BadgeId = badgeId;
        }
    }
}
