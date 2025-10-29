using MediatR;
using NekoViBE.Application.Common.DTOs.Badge;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Badge.Queries.GetBadge
{
    public record GetBadgesQuery(BadgeFilter Filter) : IRequest<PaginationResult<BadgeItem>>;

}
