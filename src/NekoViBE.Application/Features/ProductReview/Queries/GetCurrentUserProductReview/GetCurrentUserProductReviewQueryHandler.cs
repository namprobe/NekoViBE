// File: Application/Features/ProductReview/Queries/GetCurrentUserProductReview/GetCurrentUserProductReviewQueryHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System.Linq.Expressions;
using LinqKit;

namespace NekoViBE.Application.Features.ProductReview.Queries.GetCurrentUserProductReview
{
    public class GetCurrentUserProductReviewQueryHandler
        : IRequestHandler<GetCurrentUserProductReviewQuery, Result<ProductReviewResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCurrentUserProductReviewQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetCurrentUserProductReviewQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCurrentUserProductReviewQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ProductReviewResponse>> Handle(GetCurrentUserProductReviewQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();

                Expression<Func<Domain.Entities.ProductReview, bool>> predicate = x =>
                    x.UserId == userId.Value &&
                    x.ProductId == request.ProductId &&
                    !x.IsDeleted;

                if (request.OrderId.HasValue)
                {
                    predicate = predicate.And(x => x.OrderId == request.OrderId.Value);
                }

                var entity = await _unitOfWork.Repository<Domain.Entities.ProductReview>()
                    .GetFirstOrDefaultAsync(
                        predicate: predicate,
                        includes: new Expression<Func<Domain.Entities.ProductReview, object>>[]
                        {
                            x => x.User
                        });

                if (entity == null)
                {
                    _logger.LogInformation(
                        "No review found for current user {UserId} on product {ProductId}, order {OrderId}",
                        userId, request.ProductId, request.OrderId ?? (object)"any");

                    return Result<ProductReviewResponse>.Failure("Review not found", ErrorCodeEnum.NotFound);
                }

                var response = _mapper.Map<ProductReviewResponse>(entity);
                return Result<ProductReviewResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user's review for product {ProductId}", request.ProductId);
                return Result<ProductReviewResponse>.Failure("Error retrieving review", ErrorCodeEnum.InternalError);
            }
        }
    }
}