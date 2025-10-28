using MediatR;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductReview.Commands.UpdateProductReview
{
    public record UpdateProductReviewCommand(Guid Id, ProductReviewRequest Request) : IRequest<Result>;
}
