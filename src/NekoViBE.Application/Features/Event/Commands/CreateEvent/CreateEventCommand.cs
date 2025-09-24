using MediatR;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Event.Commands.CreateEvent
{
    public record CreateEventCommand(EventRequest Request) : IRequest<Result>;
}
