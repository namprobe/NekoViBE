using MediatR;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ProductImage.Queries.GetProductImage
{
    public record GetProductImageQuery(Guid Id) : IRequest<Result<ProductImageResponse>>;
}
