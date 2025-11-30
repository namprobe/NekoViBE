using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserBadge.Queries.GetUserBadgeWallet
{
    public class GetUserBadgeWalletQuery : IRequest<Result<object>>
    {
        public Guid? UserId { get; set; }
        public string? Filter { get; set; } // "unlocked" or "all"

        public GetUserBadgeWalletQuery(Guid? userId, string? filter = "unlocked")
        {
            UserId = userId;
            Filter = filter;
        }
    }
}
