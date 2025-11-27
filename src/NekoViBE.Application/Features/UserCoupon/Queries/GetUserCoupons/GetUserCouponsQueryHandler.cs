using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserCoupon;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;

namespace NekoViBE.Application.Features.UserCoupon.Queries.GetUserCoupons;

public class GetUserCouponsQueryHandler : IRequestHandler<GetUserCouponsQuery, PaginationResult<UserCouponItem>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUserCouponsQueryHandler> _logger;

    public GetUserCouponsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<GetUserCouponsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PaginationResult<UserCouponItem>> Handle(GetUserCouponsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return PaginationResult<UserCouponItem>.Failure("Not authorized", ErrorCodeEnum.Unauthorized);
            }

            if (request.Filter.IsCurrentUser is true)
            {
                request.Filter.UserId = userId;
            }

            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var page = request.Filter.Page < 1 ? 1 : request.Filter.Page;
            var pageSize = request.Filter.PageSize < 1 ? 10 : request.Filter.PageSize;

            var (userCoupons, totalCount) = await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
                .GetPagedAsync(page, pageSize, predicate, orderBy, request.Filter.IsAscending ?? false, x => x.Coupon);

            var items = _mapper.Map<List<UserCouponItem>>(userCoupons);

            return PaginationResult<UserCouponItem>.Success(items, page, pageSize, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user coupons with filter {@Filter}", request.Filter);
            return PaginationResult<UserCouponItem>.Failure("Error getting user coupons", ErrorCodeEnum.InternalError);
        }
    }
}

