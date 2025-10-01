using MediatR;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductTag.Commands.CreateProductTag
{
    public record CreateProductTagCommand(ProductTagRequest Request) : IRequest<Result>;
}
