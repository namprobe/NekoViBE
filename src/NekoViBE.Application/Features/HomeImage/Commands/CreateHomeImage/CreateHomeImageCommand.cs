using MediatR;
using NekoViBE.Application.Common.DTOs.HomeImage;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.HomeImage.Commands.CreateHomeImage
{
    public record CreateHomeImageCommand(HomeImageRequest Request) : IRequest<Result>;
}
