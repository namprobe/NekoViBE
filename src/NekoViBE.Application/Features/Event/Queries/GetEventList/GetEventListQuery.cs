using MediatR;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Event.Queries.GetEventList
{
    public record GetEventListQuery(EventFilter Filter) : IRequest<PaginationResult<EventItem>>;
}
