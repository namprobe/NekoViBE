using MediatR;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductReview.Queries.GetProductReview
{
    public record GetProductReviewQuery(Guid Id) : IRequest<Result<ProductReviewResponse>>;
}

