using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MediatR;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;
using PaymentService.Application.Commons.Models.Momo;

namespace NekoViBE.Application.Features.Order.Commands.PlaceOrder;

    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Result<PlaceOrderResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory; 
    private readonly IShippingServiceFactory _shippingServiceFactory;
    private readonly IServiceProvider _serviceProvider;

    public PlaceOrderCommandHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPaymentGatewayFactory paymentGatewayFactory,
        IShippingServiceFactory shippingServiceFactory,
        IServiceProvider serviceProvider)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _paymentGatewayFactory = paymentGatewayFactory;
        _shippingServiceFactory = shippingServiceFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<PlaceOrderResponse>> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                throw new UnauthorizedAccessException("User is not valid");
            }

            var request = command.Request;
            var paymentMethod = await _unitOfWork.Repository<Domain.Entities.PaymentMethod>()
                .GetFirstOrDefaultAsync(x => x.Id == request.PaymentMethodId && x.Status == EntityStatusEnum.Active);
            if (paymentMethod is null)
            {
                throw new KeyNotFoundException("Payment method not found or invalid");
            }

            if (paymentMethod.IsOnlinePayment && !Enum.IsDefined(typeof(PaymentGatewayType), paymentMethod.Name))
            {
                throw new ArgumentException("Invalid online payment method");
            }

            var newOrder = _mapper.Map<Domain.Entities.Order>(request);
            var newOrderItems = new List<Domain.Entities.OrderItem>();
            var productNames = new List<string>(); // Store product names for metadata
            decimal productDiscountAmount = 0m;
            decimal couponDiscountAmount = 0m;
            decimal shippingDiscountAmount = 0m;
            decimal shippingAmount = 0m;
            var appliedCoupons = new List<Domain.Entities.Coupon>();
            var appliedUserCoupons = new List<Domain.Entities.UserCoupon>();
            string? paymentUrl = null;
            PaymentGatewayType? paymentGatewayType = null;
            Domain.Entities.OrderShippingMethod? orderShippingMethod = null;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                //1. Kiểm tra xem mua hàng qua cart hay là mua ngay 1 sản phẩm
                if (request.ProductId is not null && request.Quantity is not null)
                {
                    var orderItem = new Domain.Entities.OrderItem();
                    var product = await _unitOfWork.Repository<Domain.Entities.Product>()
                        .GetFirstOrDefaultAsync(x => x.Id == request.ProductId.Value);
                    if (product is null)
                    {
                        throw new KeyNotFoundException("Product not found");
                    }
                    var (isAvailable, errorMessage) = IsProductValid(product, request.Quantity.Value);
                    if (!isAvailable)
                    {
                        throw new ArgumentException(errorMessage ?? "Product is not valid");
                    }
                    orderItem.ProductId = request.ProductId.Value;
                    orderItem.Quantity = request.Quantity.Value;
                    var (unitPrice, discountAmount) = CalculateProductPricing(product, orderItem.Quantity);
                    orderItem.UnitPrice = unitPrice;
                    orderItem.DiscountAmount = discountAmount;
                    productDiscountAmount += discountAmount;
                    orderItem.Status = EntityStatusEnum.Active;
                    orderItem.InitializeEntity(userId.Value);
                    //Apply discount if any (future)
                    newOrderItems.Add(orderItem);
                    productNames.Add(product.Name); // Store product name for metadata
                    //update product stock
                    product.StockQuantity -= request.Quantity.Value;
                    _unitOfWork.Repository<Domain.Entities.Product>().Update(product);
                }
                else
                {
                    // Nếu là mua hàng qua cart thì lấy các sản phẩm từ cart
                    var cart = await _unitOfWork.Repository<Domain.Entities.ShoppingCart>()
                        .GetFirstOrDefaultAsync(x => x.UserId == userId.Value);
                    if (cart is null)
                        throw new KeyNotFoundException("Cart not found");
                    var cartItems = await _unitOfWork.Repository<Domain.Entities.CartItem>()
                        .FindAsync(x => x.CartId == cart.Id, x => x.Product);
                    if (cartItems is null || !cartItems.Any())
                        throw new ArgumentException("Cannot place order with empty cart");

                    foreach (var cartItem in cartItems)
                    {
                        var (isAvailable, errorMessage) = IsProductValid(cartItem.Product, cartItem.Quantity);
                        if (!isAvailable)
                        {
                            throw new ArgumentException(errorMessage ?? "Product is not valid");
                        }
                        var orderItem = new Domain.Entities.OrderItem();
                        orderItem.ProductId = cartItem.ProductId;
                        orderItem.Quantity = cartItem.Quantity;
                        var (unitPrice, discountAmount) = CalculateProductPricing(cartItem.Product, orderItem.Quantity);
                        orderItem.UnitPrice = unitPrice;
                        orderItem.DiscountAmount = discountAmount;
                        productDiscountAmount += discountAmount;
                        //update product stock
                        cartItem.Product.StockQuantity -= cartItem.Quantity;
                        _unitOfWork.Repository<Domain.Entities.Product>().Update(cartItem.Product);
                        //Apply discount if any (future)
                        orderItem.Status = EntityStatusEnum.Active;
                        orderItem.InitializeEntity(userId.Value);
                        newOrderItems.Add(orderItem);
                        productNames.Add(cartItem.Product.Name); // Store product name for metadata
                    }
                    _unitOfWork.Repository<Domain.Entities.CartItem>().DeleteRange(cartItems);
                }

                // Tính TotalAmount từ OrderItems (chưa gán vào Order để tránh tracking sớm)
                newOrder.TotalAmount = newOrderItems.Sum(x => x.UnitPrice * x.Quantity);

                var amountAfterProductDiscount = newOrder.TotalAmount - productDiscountAmount;
                if (request.UserCouponIds != null &&
                    request.UserCouponIds.Any() &&
                    amountAfterProductDiscount > 0)
                {
                    // Validate và áp dụng coupons: Mỗi loại chỉ được chọn 1 voucher
                    var couponTypeGroups = new Dictionary<DiscountTypeEnum, (Domain.Entities.Coupon coupon, Domain.Entities.UserCoupon userCoupon, decimal discountAmount)>();
                    
                    foreach (var userCouponId in request.UserCouponIds.Distinct())
                    {
                        var couponResult = await ValidateUserCouponAsync(
                            userCouponId,
                            amountAfterProductDiscount,
                            userId.Value,
                            cancellationToken);

                        var discountType = couponResult.coupon.DiscountType;
                        
                        // Kiểm tra xem đã có coupon cùng loại chưa
                        if (couponTypeGroups.ContainsKey(discountType))
                        {
                            throw new ArgumentException(
                                $"Chỉ được chọn 1 voucher cho mỗi loại. Đã phát hiện nhiều voucher cùng loại {discountType}");
                        }
                        
                        couponTypeGroups[discountType] = couponResult;
                    }

                    // Áp dụng các coupon đã validate
                    foreach (var (discountType, couponResult) in couponTypeGroups)
                    {
                        appliedCoupons.Add(couponResult.coupon);
                        appliedUserCoupons.Add(couponResult.userCoupon);

                        if (discountType != DiscountTypeEnum.FreeShipping)
                        {
                            couponDiscountAmount += couponResult.discountAmount;
                        }
                    }

                    couponDiscountAmount = Math.Min(couponDiscountAmount, amountAfterProductDiscount);
                }

                newOrder.DiscountAmount = productDiscountAmount + couponDiscountAmount;
                newOrder.ShippingAmount = 0m; // Will be updated after shipping calculation
                newOrder.FinalAmount = Math.Max(0, newOrder.TotalAmount - newOrder.DiscountAmount);
                newOrder.UserId = userId.Value;
                newOrder.InitializeEntity(userId.Value);

                //2. init payment
                var payment = new Domain.Entities.Payment();
                payment.OrderId = newOrder.Id;
                payment.PaymentMethodId = paymentMethod.Id;
                payment.Amount = newOrder.FinalAmount;
                payment.Status = EntityStatusEnum.Active;
                payment.InitializeEntity(userId.Value);

                if (paymentMethod.IsOnlinePayment)
                {
                    paymentGatewayType = Enum.Parse<PaymentGatewayType>(paymentMethod.Name);
                    var paymentGatewayService = _paymentGatewayFactory.GetPaymentGatewayService(paymentGatewayType.Value);
                    
                    // Build metadata for payment request (especially for Momo)
                    var metadata = new Dictionary<string, string>();
                    
                    // Get user information
                    var user = await _unitOfWork.Repository<Domain.Entities.AppUser>()
                        .GetFirstOrDefaultAsync(x => x.Id == userId.Value);
                    if (user != null)
                    {
                        var customerName = $"{user.FirstName} {user.LastName}".Trim();
                        if (!string.IsNullOrEmpty(customerName))
                        {
                            metadata["CustomerName"] = customerName;
                        }
                    }
                    
                    // Add product names to metadata
                    if (productNames.Any())
                    {
                        metadata["ProductNames"] = string.Join(", ", productNames);
                    }
                    
                    // Add order amounts
                    metadata["TotalAmount"] = newOrder.TotalAmount.ToString("N0");
                    metadata["FinalAmount"] = newOrder.FinalAmount.ToString("N0");
                    metadata["OrderId"] = newOrder.Id.ToString();
                    
                    // Add order creation date (UTC ISO format)
                    metadata["CreatedAt"] = (newOrder.CreatedAt ?? DateTime.UtcNow).ToString("O");
                    
                    var paymentIntent = await paymentGatewayService.CreatePaymentIntentAsync(new PaymentRequest
                    {
                        OrderId = newOrder.Id.ToString(),
                        Amount = newOrder.FinalAmount,
                        Currency = "VND",
                        Description = "Thanh Toan Don Hang Cho " + newOrder.Id,
                        Metadata = metadata
                    });

                    paymentUrl = paymentGatewayType switch
                    {
                        PaymentGatewayType.Momo => (paymentIntent as MomoResponse)?.PayUrl,
                        PaymentGatewayType.VnPay => paymentIntent as string,
                        _ => paymentIntent?.ToString()
                    };

                    if (string.IsNullOrWhiteSpace(paymentUrl))
                    {
                        throw new InvalidOperationException($"Failed to generate payment url for {paymentGatewayType.Value}");
                    }
                }

                if (appliedCoupons.Any())
                {
                    await AttachCouponsToOrderAsync(newOrder, appliedCoupons, appliedUserCoupons);
                }

                //3. Handle Shipping (Client đã tính phí và truyền lên)
                // Client sẽ gọi API calculate-fee trước, rồi truyền ShippingAmount vào request
                if (request.ShippingMethodId.HasValue)
                {
                    // Validate shipping method exists and is active
                    var shippingMethod = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                        .GetFirstOrDefaultAsync(x => x.Id == request.ShippingMethodId.Value && x.Status == EntityStatusEnum.Active);
                    
                    if (shippingMethod == null)
                    {
                        throw new KeyNotFoundException("Shipping method not found or invalid");
                    }

                    // Validate user address if provided
                    if (request.UserAddressId.HasValue)
                    {
                        var userAddress = await _unitOfWork.Repository<Domain.Entities.UserAddress>()
                            .GetFirstOrDefaultAsync(x => x.Id == request.UserAddressId.Value && x.UserId == userId.Value);
                        
                        if (userAddress == null)
                        {
                            throw new KeyNotFoundException("User address not found");
                        }
                    }

                    // Use shipping amount from client request (client đã tính phí trước)
                    shippingAmount = request.ShippingAmount ?? 0m;
                    
                    // Validate shipping amount is not negative
                    if (shippingAmount < 0)
                    {
                        throw new ArgumentException("Shipping amount cannot be negative");
                    }

                    // Create OrderShippingMethod record (without tracking number - will be updated after shipping order is created)
                    orderShippingMethod = new Domain.Entities.OrderShippingMethod
                    {
                        OrderId = newOrder.Id,
                        ShippingMethodId = shippingMethod.Id,
                        TrackingNumber = null, // Will be set after shipping order is created
                        Status = EntityStatusEnum.Active
                    };
                    orderShippingMethod.InitializeEntity(userId.Value);
                    await _unitOfWork.Repository<Domain.Entities.OrderShippingMethod>().AddAsync(orderShippingMethod);

                    // Apply freeship coupons if available
                    var freeShippingCoupons = appliedCoupons
                        .Where(c => c.DiscountType == DiscountTypeEnum.FreeShipping)
                        .ToList();

                    if (freeShippingCoupons.Any())
                    {
                        var remainingShipping = shippingAmount;
                        foreach (var coupon in freeShippingCoupons)
                        {
                            if (remainingShipping <= 0)
                            {
                                break;
                            }

                            if (coupon.DiscountValue > 0)
                            {
                                var applied = Math.Min(remainingShipping, coupon.DiscountValue);
                                shippingDiscountAmount += applied;
                                remainingShipping -= applied;
                            }
                            else
                            {
                                shippingDiscountAmount += remainingShipping;
                                remainingShipping = 0;
                            }
                        }
                    }

                    // Update order with shipping amount and discounts
                    var totalDiscountAmount = productDiscountAmount + couponDiscountAmount + shippingDiscountAmount;
                    var userShippingPayable = Math.Max(0, shippingAmount - shippingDiscountAmount);

                    newOrder.ShippingAmount = shippingAmount;
                    newOrder.DiscountAmount = totalDiscountAmount;
                    newOrder.FinalAmount = Math.Max(0, newOrder.TotalAmount - (productDiscountAmount + couponDiscountAmount) + userShippingPayable);
                    //_unitOfWork.Repository<Domain.Entities.Order>().Update(newOrder);
                    
                    // Only create shipping order immediately for COD (offline payment)
                    // For online payment, shipping order will be created after payment success
                    if (!paymentMethod.IsOnlinePayment)
                    {
                        await CreateShippingOrderAfterPaymentAsync(
                            newOrder,
                            orderShippingMethod,
                            userId.Value,
                            paymentMethod,
                            cancellationToken);
                    }
                }
                else
                {
                    // No shipping method selected, shipping amount should be 0
                    shippingAmount = 0m;
                    newOrder.ShippingAmount = 0m;
                    newOrder.DiscountAmount = productDiscountAmount + couponDiscountAmount;
                    newOrder.FinalAmount = Math.Max(0, newOrder.TotalAmount - newOrder.DiscountAmount);
                    // _unitOfWork.Repository<Domain.Entities.Order>().Update(newOrder);
                }
                newOrder.OrderItems = newOrderItems;
                await _unitOfWork.Repository<Domain.Entities.Order>().AddAsync(newOrder);
                await _unitOfWork.Repository<Domain.Entities.Payment>().AddAsync(payment);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var response = new PlaceOrderResponse
                {
                    OrderId = newOrder.Id,
                    PaymentGateway = paymentGatewayType,
                    PaymentUrl = paymentUrl,
                    CreatedAt = newOrder.CreatedAt ?? DateTime.UtcNow,
                    FinalAmount = newOrder.FinalAmount
                };

                UserActionHelper.LogUserActionAsync(
                    _serviceProvider,
                    userId.Value,
                    UserActionEnum.Create,
                    newOrder.Id,
                    nameof(Domain.Entities.Order),
                    $"Order placed via {paymentMethod.Name}",
                    _currentUserService.IPAddress,
                    newValue: new
                    {
                        newOrder.FinalAmount,
                        PaymentGateway = paymentGatewayType?.ToString(),
                        paymentUrl
                    },
                    cancellationToken: cancellationToken);

                return Result<PlaceOrderResponse>.Success(
                    response,
                    "Order placed successfully");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch
        {
            throw;
        }
    }

    private (bool isAvailable, string? errorMessage) IsProductValid(Domain.Entities.Product product, int quantity)
    {
        if (product.Status != EntityStatusEnum.Active)
        {
            return (false, "Product " + product.Name + " is not active");
        }
        if (product.StockQuantity == 0)
        {
            return (false, "Product " + product.Name + " is out of stock");
        }
        if (product.StockQuantity < quantity)
        {
            return (false, "Product " + product.Name + " has not enough stock");
        }
        return (true, null);
    }

    private (decimal unitPrice, decimal discountAmount) CalculateProductPricing(Domain.Entities.Product product, int quantity)
    {
        var unitPrice = product.Price;
        decimal discountAmount = 0m;

        // Chỉ tính discount khi có DiscountPrice hợp lệ (có giá trị, > 0, và <= Price)
        if (product.DiscountPrice.HasValue 
            && product.DiscountPrice.Value > 0  
            && product.DiscountPrice.Value <= product.Price)
        {
            var perItemDiscount = product.Price - product.DiscountPrice.Value;
            discountAmount = perItemDiscount * quantity;
        }

        return (unitPrice, discountAmount);
    }

    private async Task<(Domain.Entities.Coupon coupon, Domain.Entities.UserCoupon userCoupon, decimal discountAmount)> ValidateUserCouponAsync(
        Guid userCouponId,
        decimal orderAmountAfterProductDiscount,
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Load UserCoupon với Coupon navigation property
        var userCoupon = await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
            .GetFirstOrDefaultAsync(x => x.Id == userCouponId, x => x.Coupon);
        
        if (userCoupon is null)
            throw new ArgumentException("User coupon not found");

        // Kiểm tra UserCoupon thuộc về user hiện tại
        if (userCoupon.UserId != userId)
            throw new ArgumentException("This coupon does not belong to the current user");

        // Kiểm tra UserCoupon chưa được sử dụng
        if (userCoupon.UsedDate.HasValue)
            throw new ArgumentException("This coupon has already been used");

        // Load Coupon nếu chưa được load
        if (userCoupon.Coupon is null)
        {
            userCoupon.Coupon = await _unitOfWork.Repository<Domain.Entities.Coupon>()
                .GetFirstOrDefaultAsync(x => x.Id == userCoupon.CouponId);
        }

        var coupon = userCoupon.Coupon;
        if (coupon is null)
            throw new ArgumentException("Coupon not found");

        // Kiểm tra coupon status
        if (coupon.Status != EntityStatusEnum.Active)
            throw new ArgumentException("Coupon is not active");

        // Kiểm tra thời gian hiệu lực
        var now = DateTime.UtcNow;
        if (now < coupon.StartDate || now > coupon.EndDate)
        {
            throw new ArgumentException("Coupon is not valid at this time");
        }

        // Kiểm tra usage limit
        if (coupon.UsageLimit.HasValue && coupon.CurrentUsage >= coupon.UsageLimit.Value)
        {
            throw new ArgumentException("Coupon usage limit exceeded");
        }

        // Kiểm tra minimum order amount
        if (orderAmountAfterProductDiscount < coupon.MinOrderAmount)
        {
            throw new ArgumentException($"Minimum order amount of {coupon.MinOrderAmount:N0} is required for this coupon");
        }

        // Tính discount amount
        var discountAmount = coupon.DiscountType switch
        {
            DiscountTypeEnum.Percentage => orderAmountAfterProductDiscount * (coupon.DiscountValue / 100),
            DiscountTypeEnum.Fixed => coupon.DiscountValue,
            _ => 0m
        };

        // Đảm bảo discount không vượt quá order amount
        if (discountAmount > orderAmountAfterProductDiscount)
        {
            discountAmount = orderAmountAfterProductDiscount;
        }

        return (coupon, userCoupon, discountAmount);
    }

    private Task AttachCouponsToOrderAsync(
        Domain.Entities.Order order,
        IReadOnlyList<Domain.Entities.Coupon> coupons,
        IReadOnlyList<Domain.Entities.UserCoupon> userCoupons)
    {
        if (coupons.Count == 0 || userCoupons.Count == 0)
    {
            return Task.CompletedTask;
        }

        var pairCount = Math.Min(coupons.Count, userCoupons.Count);
        for (var i = 0; i < pairCount; i++)
        {
            var userCoupon = userCoupons[i];
        userCoupon.OrderId = order.Id;
        userCoupon.UsedDate = DateTime.UtcNow;
        userCoupon.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Domain.Entities.UserCoupon>().Update(userCoupon);

            order.UserCoupons ??= new List<Domain.Entities.UserCoupon>();
        order.UserCoupons.Add(userCoupon);
        }

        return Task.CompletedTask;
    }


    /// <summary>
    /// Create actual shipping order with GHN after payment success
    /// </summary>
    private async Task<Domain.Entities.OrderShippingMethod?> CreateShippingOrderAfterPaymentAsync(
        Domain.Entities.Order order,
        Domain.Entities.OrderShippingMethod orderShippingMethod,
        Guid userId,
        Domain.Entities.PaymentMethod paymentMethod,
        CancellationToken cancellationToken)
    {
        try
        {
            // If tracking number already exists, shipping order was already created
            if (!string.IsNullOrWhiteSpace(orderShippingMethod.TrackingNumber))
            {
                return orderShippingMethod;
            }

            var shippingMethod = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                .GetFirstOrDefaultAsync(x => x.Id == orderShippingMethod.ShippingMethodId);
            
            if (shippingMethod == null || !Enum.TryParse<ShippingProviderType>(shippingMethod.Name, out var providerType) || 
                providerType != ShippingProviderType.GHN)
            {
                return orderShippingMethod;
            }

            // Get user address
            var userAddress = await _unitOfWork.Repository<Domain.Entities.UserAddress>()
                .GetFirstOrDefaultAsync(
                    x => x.UserId == order.UserId && x.IsDefault);
            
            if (userAddress == null ||
                !userAddress.ProvinceId.HasValue ||
                !userAddress.DistrictId.HasValue ||
                string.IsNullOrWhiteSpace(userAddress.WardCode) ||
                string.IsNullOrWhiteSpace(userAddress.ProvinceName) ||
                string.IsNullOrWhiteSpace(userAddress.DistrictName) ||
                string.IsNullOrWhiteSpace(userAddress.WardName))
            {
                throw new ArgumentException("User address is missing required GHN identifiers");
            }

            var ghnService = _shippingServiceFactory.GetShippingService(ShippingProviderType.GHN);

            // Load order items
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                var orderItems = await _unitOfWork.Repository<Domain.Entities.OrderItem>()
                    .FindAsync(x => x.OrderId == order.Id, x => x.Product);
                order.OrderItems = orderItems.ToList();
            }

            var shippingItems = order.OrderItems?.Select(item => new ShippingOrderItem
            {
                Name = item.Product?.Name ?? "Product",
                Code = item.ProductId.ToString(),
                Quantity = item.Quantity,
                Price = (int)item.UnitPrice,
                Weight = 500,
                Length = 20,
                Width = 20,
                Height = 20
            }).ToList();

            var shippingOrderRequest = new ShippingOrderRequest
            {
                ClientOrderCode = order.Id.ToString(),
                ToName = userAddress.FullName,
                ToPhone = userAddress.PhoneNumber ?? string.Empty,
                ToAddress = userAddress.Address,
                ToWardName = userAddress.WardName,
                ToDistrictName = userAddress.DistrictName,
                ToProvinceName = userAddress.ProvinceName,
                PaymentTypeId = paymentMethod.IsOnlinePayment ? 1 : 2,
                ServiceTypeId = 2,
                RequiredNote = "KHONGCHOXEMHANG",
                Note = order.Notes,
                Weight = 500,
                Length = 20,
                Width = 20,
                Height = 20,
                CodAmount = !paymentMethod.IsOnlinePayment ? (int)order.FinalAmount : null,
                InsuranceValue = (int)order.FinalAmount,
                Items = shippingItems
            };

            // Preview order
            var previewResult = await ghnService.PreviewOrderAsync(shippingOrderRequest);
            if (!previewResult.IsSuccess)
            {
                throw new InvalidOperationException($"Failed to preview shipping order: {previewResult.Message}");
            }

            // Create shipping order
            var createResult = await ghnService.CreateOrderAsync(shippingOrderRequest);
            if (!createResult.IsSuccess || createResult.Data == null)
            {
                throw new InvalidOperationException($"Failed to create shipping order: {createResult.Message}");
            }

            // Update OrderShippingMethod with tracking number
            orderShippingMethod.TrackingNumber = createResult.Data.OrderCode;
            orderShippingMethod.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Domain.Entities.OrderShippingMethod>().Update(orderShippingMethod);

            return orderShippingMethod;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create shipping order after payment: {ex.Message}", ex);
        }
    }
}
