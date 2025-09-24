using MediatR;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ProductImage.Queries.GetProductImageList
{
    public record GetProductImageListQuery(ProductImageFilter Filter)
        : IRequest<PaginationResult<ProductImageItem>>;
}
