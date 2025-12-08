// File: Application/Features/ProductReview/Queries/GetCurrentUserProductReview/GetCurrentUserProductReviewQuery.cs
using MediatR;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ProductReview.Queries.GetCurrentUserProductReview
{
    public record GetCurrentUserProductReviewQuery(Guid ProductId, Guid? OrderId = null)
        : IRequest<Result<ProductReviewResponse>>;
}