using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Coupon.Queries.GetCoupons
{
    public class GetCouponListQueryHandler : IRequestHandler<GetCouponListQuery, PaginationResult<CouponItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCouponListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetCouponListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCouponListQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<CouponItem>> Handle(GetCouponListQuery request, CancellationToken cancellationToken)
        {
            var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
            if (!isValid || currentUserId == null)
            {
                return PaginationResult<CouponItem>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
            }

            try
            {
                // Use extension methods for filtering and ordering
                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                var (coupons, totalCount) = await _unitOfWork.Repository<Domain.Entities.Coupon>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending);

                var couponItems = _mapper.Map<List<CouponItem>>(coupons);

                _logger.LogInformation("Coupon list retrieved successfully by {CurrentUser}, TotalItems: {TotalCount}",
                    currentUserId, totalCount);

                return PaginationResult<CouponItem>.Success(
                    couponItems,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coupon list");
                return PaginationResult<CouponItem>.Failure("An error occurred while retrieving coupons", ErrorCodeEnum.InternalError);
            }
        }
    }
}
