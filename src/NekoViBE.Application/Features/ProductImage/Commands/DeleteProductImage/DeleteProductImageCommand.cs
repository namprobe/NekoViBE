using MediatR;
using NekoViBE.Application.Common.Models;
using System;

namespace NekoViBE.Application.Features.ProductImage.Commands.DeleteProductImage
{
    public record DeleteProductImageCommand(Guid Id) : IRequest<Result>;
}
