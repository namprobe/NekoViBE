using MediatR;
using NekoViBE.Application.Common.DTOs.UserBadge;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.UserBadge.Queries.GetUserBadge
{
    public record GetUserBadgesQuery(Guid UserId) : IRequest<Result<UserBadgesResponse>>;

}
