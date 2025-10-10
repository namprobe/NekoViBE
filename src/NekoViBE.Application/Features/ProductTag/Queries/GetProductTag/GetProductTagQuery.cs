using MediatR;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ProductTag.Queries.GetProductTag
{
    public record GetProductTagQuery(Guid Id) : IRequest<Result<ProductTagResponse>>;
}
