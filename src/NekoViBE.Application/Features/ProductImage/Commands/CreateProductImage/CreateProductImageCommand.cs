using MediatR;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductImage.Commands.CreateProductImage
{
    public record CreateProductImageCommand(ProductImageRequest Request) : IRequest<Result>;
}
