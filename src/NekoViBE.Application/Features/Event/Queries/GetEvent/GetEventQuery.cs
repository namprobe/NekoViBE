using MediatR;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Event.Queries.GetEvent
{
    public record GetEventQuery(Guid Id) : IRequest<Result<EventResponse>>;
}
