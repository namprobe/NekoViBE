using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.Models.GHN;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.ShippingMethod.Queries.CalculateShippingFee;

public class CalculateShippingFeeQueryHandler : IRequestHandler<CalculateShippingFeeQuery, Result<ShippingFeeResult>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShippingServiceFactory _shippingServiceFactory;
    private readonly ILogger<CalculateShippingFeeQueryHandler> _logger;
    private readonly GHNSettings _ghnSettings;
    public CalculateShippingFeeQueryHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IShippingServiceFactory shippingServiceFactory,
        ILogger<CalculateShippingFeeQueryHandler> logger,
        IOptions<GHNSettings> ghnSettings)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _shippingServiceFactory = shippingServiceFactory;
        _logger = logger;
        _ghnSettings = ghnSettings.Value;
    }

    public async Task<Result<ShippingFeeResult>> Handle(CalculateShippingFeeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return Result<ShippingFeeResult>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }

            var req = request.Request;

            // Validate shipping method
            var shippingMethod = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                .GetFirstOrDefaultAsync(x => x.Id == req.ShippingMethodId && x.Status == EntityStatusEnum.Active);

            if (shippingMethod == null)
            {
                return Result<ShippingFeeResult>.Failure("Shipping method not found or invalid", ErrorCodeEnum.NotFound);
            }

            // Validate user address
            var userAddress = await _unitOfWork.Repository<Domain.Entities.UserAddress>()
                .GetFirstOrDefaultAsync(
                    x => x.Id == req.UserAddressId && x.UserId == userId.Value);

            if (userAddress == null)
            {
                return Result<ShippingFeeResult>.Failure("User address not found", ErrorCodeEnum.NotFound);
            }

            // Validate address has required GHN fields
            if (!userAddress.ProvinceId.HasValue ||
                !userAddress.DistrictId.HasValue ||
                string.IsNullOrWhiteSpace(userAddress.WardCode))
            {
                return Result<ShippingFeeResult>.Failure(
                    "User address is missing required GHN identifiers (provinceId, districtId, wardCode)",
                    ErrorCodeEnum.ValidationFailed);
            }

            // Check if shipping method is GHN
            if (!Enum.TryParse<ShippingProviderType>(shippingMethod.Name, out var providerType) || 
                providerType != ShippingProviderType.GHN)
            {
                // For non-GHN shipping methods, return fixed cost
                return Result<ShippingFeeResult>.Success(new ShippingFeeResult
                {
                    IsSuccess = true,
                    Message = "Shipping fee calculated",
                    Data = new ShippingFeeData
                    {
                        Total = (int)shippingMethod.Cost,
                        ServiceFee = (int)shippingMethod.Cost,
                        InsuranceFee = 0,
                        PickStationFee = 0,
                        CouponValue = 0,
                        R2SFee = 0,
                        DocumentReturn = 0,
                        DoubleCheck = 0,
                        CodFee = 0,
                        PickRemoteAreasFee = 0,
                        DeliverRemoteAreasFee = 0,
                        CodFailedFee = 0,
                        ReturnAgainFee = 0
                    }
                });
            }

            // Get GHN service
            var ghnService = _shippingServiceFactory.GetShippingService(ShippingProviderType.GHN);

            // Get shop district and ward code
            var fromDistrictId = _ghnSettings.ShopDistrictId;
            var fromWardCode = _ghnSettings.ShopWardCode;

            // Calculate total order amount and dimensions
            decimal totalOrderAmount = 0m;
            int totalWeight = 0;
            int totalLength = 0;
            int totalWidth = 0;
            int totalHeight = 0;

            // Case 1: Buy now (ProductId provided)
            if (req.ProductId.HasValue)
            {
                if (!req.Quantity.HasValue || req.Quantity.Value <= 0)
                {
                    return Result<ShippingFeeResult>.Failure(
                        "Quantity is required and must be greater than zero when ProductId is provided",
                        ErrorCodeEnum.ValidationFailed);
                }

                var product = await _unitOfWork.Repository<Domain.Entities.Product>()
                    .GetFirstOrDefaultAsync(x => x.Id == req.ProductId.Value);

                if (product == null)
                {
                    return Result<ShippingFeeResult>.Failure("Product not found", ErrorCodeEnum.NotFound);
                }

                if (product.Status != EntityStatusEnum.Active)
                {
                    return Result<ShippingFeeResult>.Failure("Product is not active", ErrorCodeEnum.ValidationFailed);
                }

                var unitPrice = product.DiscountPrice ?? product.Price;
                totalOrderAmount = unitPrice * req.Quantity.Value;

                // Create shipping item
                var itemWeight = 500; // Default weight per item
                var itemLength = 20; // Default dimensions
                var itemWidth = 20;
                var itemHeight = 20;

                totalWeight = itemWeight * req.Quantity.Value;
                totalLength = itemLength;
                totalWidth = itemWidth;
                totalHeight = itemHeight * req.Quantity.Value; // Stack items vertically
            }
            // Case 2: Buy from cart (no ProductId provided)
            else
            {
                var cart = await _unitOfWork.Repository<Domain.Entities.ShoppingCart>()
                    .GetFirstOrDefaultAsync(x => x.UserId == userId.Value);

                if (cart == null)
                {
                    return Result<ShippingFeeResult>.Failure("Cart not found", ErrorCodeEnum.NotFound);
                }

                var cartItems = await _unitOfWork.Repository<Domain.Entities.CartItem>()
                    .FindAsync(x => x.CartId == cart.Id, x => x.Product);

                if (cartItems == null || !cartItems.Any())
                {
                    return Result<ShippingFeeResult>.Failure("Cart items not found", ErrorCodeEnum.NotFound);
                }

                foreach (var cartItem in cartItems)
                {
                    if (cartItem.Product == null)
                    {
                        continue;
                    }

                    if (cartItem.Product.Status != EntityStatusEnum.Active)
                    {
                        continue; // Skip inactive products
                    }

                    var unitPrice = cartItem.Product.DiscountPrice ?? cartItem.Product.Price;
                    totalOrderAmount += unitPrice * cartItem.Quantity;

                    // Create shipping item
                    var itemWeight = 500; // Default weight per item
                    var itemLength = 20; // Default dimensions
                    var itemWidth = 20;
                    var itemHeight = 20;

                    totalWeight += itemWeight * cartItem.Quantity;
                    totalLength = Math.Max(totalLength, itemLength);
                    totalWidth = Math.Max(totalWidth, itemWidth);
                    totalHeight += itemHeight * cartItem.Quantity; // Stack items vertically
                }

                if (totalOrderAmount == 0)
                {
                    return Result<ShippingFeeResult>.Failure("No valid cart items found", ErrorCodeEnum.ValidationFailed);
                }
            }

            // Determine service type (2: Hàng nhẹ, 5: Hàng nặng)
            // For now, default to 2 (Hàng nhẹ), can be made configurable later
            var serviceTypeId = 2;

            // Build shipping fee request
            var feeRequest = new ShippingFeeRequest
            {
                FromDistrictId = fromDistrictId,
                FromWardCode = fromWardCode,
                ToDistrictId = userAddress.DistrictId.Value,
                ToWardCode = userAddress.WardCode,
                ServiceTypeId = serviceTypeId,
                Weight = totalWeight > 0 ? totalWeight : 500,
                Length = totalLength > 0 ? totalLength : 20,
                Width = totalWidth > 0 ? totalWidth : 20,
                Height = totalHeight > 0 ? totalHeight : 20,
                InsuranceValue = req.InsuranceValue ?? (int)totalOrderAmount,
                CodAmount = req.CodValue, // Map CodValue to CodAmount
                Coupon = req.Coupon
            };

            // Calculate fee using GHN service
            var feeResult = await ghnService.CalculateFeeAsync(feeRequest);

            if (!feeResult.IsSuccess)
            {
                return Result<ShippingFeeResult>.Failure(
                    feeResult.Message ?? "Failed to calculate shipping fee",
                    ErrorCodeEnum.InternalError);
            }

            return Result<ShippingFeeResult>.Success(feeResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping fee for request: {@Request}", request.Request);
            return Result<ShippingFeeResult>.Failure(
                $"Error calculating shipping fee: {ex.Message}",
                ErrorCodeEnum.InternalError);
        }
    }
}

