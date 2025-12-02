using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.Models.GHN;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.ShippingMethod.Queries.CalculateShippingLeadTime;

public class CalculateShippingLeadTimeQueryHandler : IRequestHandler<CalculateShippingLeadTimeQuery, Result<ShippingLeadTimeResult>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShippingServiceFactory _shippingServiceFactory;
    private readonly ILogger<CalculateShippingLeadTimeQueryHandler> _logger;
    private readonly GHNSettings _ghnSettings;

    public CalculateShippingLeadTimeQueryHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IShippingServiceFactory shippingServiceFactory,
        ILogger<CalculateShippingLeadTimeQueryHandler> logger,
        IOptions<GHNSettings> ghnSettings)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _shippingServiceFactory = shippingServiceFactory;
        _logger = logger;
        _ghnSettings = ghnSettings.Value;
    }

    public async Task<Result<ShippingLeadTimeResult>> Handle(CalculateShippingLeadTimeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return Result<ShippingLeadTimeResult>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }

            var req = request.Request;

            var shippingMethod = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                .GetFirstOrDefaultAsync(x => x.Id == req.ShippingMethodId && x.Status == EntityStatusEnum.Active);

            if (shippingMethod == null)
            {
                return Result<ShippingLeadTimeResult>.Failure("Shipping method not found or invalid", ErrorCodeEnum.NotFound);
            }

            var userAddress = await _unitOfWork.Repository<Domain.Entities.UserAddress>()
                .GetFirstOrDefaultAsync(x => x.Id == req.UserAddressId && x.UserId == userId.Value);

            if (userAddress == null)
            {
                return Result<ShippingLeadTimeResult>.Failure("User address not found", ErrorCodeEnum.NotFound);
            }

            if (!userAddress.ProvinceId.HasValue ||
                !userAddress.DistrictId.HasValue ||
                string.IsNullOrWhiteSpace(userAddress.WardCode))
            {
                return Result<ShippingLeadTimeResult>.Failure(
                    "User address is missing required GHN identifiers (provinceId, districtId, wardCode)",
                    ErrorCodeEnum.ValidationFailed);
            }

            if (!Enum.TryParse<ShippingProviderType>(shippingMethod.Name, out var providerType) ||
                providerType != ShippingProviderType.GHN)
            {
                _logger.LogInformation(
                    "[LeadTime] Using static configuration for ShippingMethodId: {ShippingMethodId}",
                    shippingMethod.Id);

                var estimatedDays = shippingMethod.EstimatedDays ?? 0;
                var leadTimeDate = estimatedDays > 0
                    ? DateTime.UtcNow.AddDays(estimatedDays)
                    : (DateTime?)null;
                long? orderDateTimestamp = leadTimeDate.HasValue
                    ? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    : null;
                long? leadTimeUnix = leadTimeDate.HasValue
                    ? new DateTimeOffset(leadTimeDate.Value).ToUnixTimeSeconds()
                    : null;

                return Result<ShippingLeadTimeResult>.Success(new ShippingLeadTimeResult
                {
                    IsSuccess = true,
                    Message = "Lead time calculated from static shipping method configuration",
                    Data = leadTimeDate.HasValue
                        ? new ShippingLeadTimeData
                        {
                            LeadTime = leadTimeDate,
                            LeadTimeUnix = leadTimeUnix,
                            OrderDateUnix = orderDateTimestamp
                        }
                        : null
                });
            }

            var ghnService = _shippingServiceFactory.GetShippingService(ShippingProviderType.GHN);

            var leadTimeRequest = new ShippingLeadTimeRequest
            {
                FromDistrictId = _ghnSettings.ShopDistrictId,
                FromWardCode = _ghnSettings.ShopWardCode,
                ToDistrictId = userAddress.DistrictId.Value,
                ToWardCode = userAddress.WardCode,
                ServiceTypeId = 2 // Default GHN service type
            };

            _logger.LogInformation(
                "[LeadTime] Calling GHN service for ShippingMethodId: {ShippingMethodId}, AddressId: {AddressId}, Request: {@LeadTimeRequest}",
                shippingMethod.Id,
                userAddress.Id,
                leadTimeRequest);

            var leadTimeResult = await ghnService.GetLeadTimeAsync(leadTimeRequest);

            if (!leadTimeResult.IsSuccess)
            {
                return Result<ShippingLeadTimeResult>.Failure(
                    leadTimeResult.Message ?? "Failed to calculate shipping lead time",
                    ErrorCodeEnum.InternalError);
            }

            if (leadTimeResult.Data == null || leadTimeResult.Data.LeadTime == null)
            {
                _logger.LogWarning(
                    "[LeadTime] GHN returned null lead time for ShippingMethodId: {ShippingMethodId}, AddressId: {AddressId}",
                    shippingMethod.Id,
                    userAddress.Id);
            }

            return Result<ShippingLeadTimeResult>.Success(leadTimeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping lead time for request: {@Request}", request.Request);
            return Result<ShippingLeadTimeResult>.Failure(
                $"Error calculating shipping lead time: {ex.Message}",
                ErrorCodeEnum.InternalError);
        }
    }
}

