using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserBadge.Command.ProcessBadgeEligibility
{
    public class ProcessBadgeEligibilityCommand : IRequest<Result>
    {
        public Guid? UserId { get; set; }

        public ProcessBadgeEligibilityCommand(Guid? userId = null)
        {
            UserId = userId;
        }
    }
}
