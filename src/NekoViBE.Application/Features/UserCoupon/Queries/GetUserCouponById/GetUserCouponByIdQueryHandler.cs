using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.UserCoupon;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.UserCoupon.Queries.GetUserCouponById;

public class GetUserCouponByIdQueryHandler : IRequestHandler<GetUserCouponByIdQuery, Result<UserCouponDetail>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUserCouponByIdQueryHandler> _logger;

    public GetUserCouponByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<GetUserCouponByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<UserCouponDetail>> Handle(GetUserCouponByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return Result<UserCouponDetail>.Failure("Not authorized", ErrorCodeEnum.Unauthorized);
            }

            var userCoupon = await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
                .GetFirstOrDefaultAsync(x => x.Id == request.Id, x => x.Coupon);

            if (userCoupon is null)
            {
                return Result<UserCouponDetail>.Failure("User coupon not found", ErrorCodeEnum.NotFound);
            }

            if (userCoupon.UserId != userId)
            {
                return Result<UserCouponDetail>.Failure("This coupon does not belong to the current user", ErrorCodeEnum.Forbidden);
            }

            var detail = _mapper.Map<UserCouponDetail>(userCoupon);
            return Result<UserCouponDetail>.Success(detail, "User coupon retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user coupon {UserCouponId}", request.Id);
            return Result<UserCouponDetail>.Failure("Error getting user coupon", ErrorCodeEnum.InternalError);
        }
    }
}

