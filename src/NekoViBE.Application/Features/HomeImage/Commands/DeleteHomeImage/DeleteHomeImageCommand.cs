using MediatR;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.HomeImage.Commands.DeleteHomeImage
{
    public record DeleteHomeImageCommand(Guid Id) : IRequest<Result>;
}
