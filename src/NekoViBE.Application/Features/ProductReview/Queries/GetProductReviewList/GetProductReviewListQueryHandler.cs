using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using NekoViBE.Domain.Entities;
using System.Linq.Expressions;

namespace NekoViBE.Application.Features.ProductReview.Queries.GetProductReviewList
{
    public class GetProductReviewListQueryHandler : IRequestHandler<GetProductReviewListQuery, PaginationResult<ProductReviewItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductReviewListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetProductReviewListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetProductReviewListQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<ProductReviewItem>> Handle(GetProductReviewListQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.ProductReview>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending,
                    includes: new Expression<Func<Domain.Entities.ProductReview, object>>[]
                    {
                        x => x.User
                    });

                var reviewItems = _mapper.Map<List<ProductReviewItem>>(items);


                foreach (var dto in reviewItems)
                {
                    // Tìm lại entity gốc tương ứng
                    var originalEntity = items.FirstOrDefault(x => x.Id == dto.Id);

                    if (originalEntity?.User != null)
                    {
                        // Ghép FirstName và LastName
                        string fullName = $"{originalEntity.User.FirstName} {originalEntity.User.LastName}".Trim();

                        // Nếu ghép ra chuỗi rỗng (chưa set tên) thì mới fallback về UserName cũ (Email)
                        if (!string.IsNullOrEmpty(fullName))
                        {
                            dto.UserName = fullName;
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} product reviews", reviewItems.Count);

                return PaginationResult<ProductReviewItem>.Success(reviewItems, request.Filter.Page, request.Filter.PageSize, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product review list with filter: {@Filter}", request.Filter);
                return PaginationResult<ProductReviewItem>.Failure("Error getting product review list", ErrorCodeEnum.InternalError);
            }
        }
    }
}