using MediatR;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Tag.Commands.UpdateTag
{
    public record UpdateTagCommand(Guid Id, TagRequest Request) : IRequest<Result>;
}
