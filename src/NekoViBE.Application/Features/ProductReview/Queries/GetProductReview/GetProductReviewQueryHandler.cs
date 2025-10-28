using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductReview.Queries.GetProductReview
{
    public class GetProductReviewQueryHandler : IRequestHandler<GetProductReviewQuery, Result<ProductReviewResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductReviewQueryHandler> _logger;

        public GetProductReviewQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetProductReviewQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ProductReviewResponse>> Handle(GetProductReviewQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.ProductReview>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Id == query.Id && !x.IsDeleted,
                        includes: new Expression<Func<Domain.Entities.ProductReview, object>>[]
                        {
                            x => x.User
                        });

                if (entity == null)
                {
                    _logger.LogWarning("Product review with ID {Id} not found", query.Id);
                    return Result<ProductReviewResponse>.Failure("Product review not found", ErrorCodeEnum.NotFound);
                }

                var response = _mapper.Map<ProductReviewResponse>(entity);
                _logger.LogInformation("Retrieved product review {Id}", query.Id);
                return Result<ProductReviewResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product review with ID {Id}", query.Id);
                return Result<ProductReviewResponse>.Failure("Error getting product review", ErrorCodeEnum.InternalError);
            }
        }
    }
}
