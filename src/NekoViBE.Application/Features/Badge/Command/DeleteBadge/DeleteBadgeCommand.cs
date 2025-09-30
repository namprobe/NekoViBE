using MediatR;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Badge.Command.DeleteBadge
{
    public record DeleteBadgeCommand(Guid Id) : IRequest<Result>;

}
