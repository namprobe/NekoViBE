using MediatR;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Product.Queries.GetProductList
{
    public record GetProductListQuery(ProductFilter Filter) : IRequest<PaginationResult<ProductItem>>;
}
