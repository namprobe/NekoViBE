using MediatR;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Event.Commands.DeleteEvent
{
    public record DeleteEventCommand(Guid Id) : IRequest<Result>;
}
