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
            var appliedCoupons = new List<(Domain.Entities.Coupon coupon, Domain.Entities.UserCoupon userCoupon, decimal discountAmount, decimal baseAmount)>();
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
                    var (unitPriceOriginal, unitPriceAfterDiscount, unitDiscountAmount, lineTotal) = CalculateProductPricing(product, orderItem.Quantity);
                    orderItem.UnitPriceOriginal = unitPriceOriginal;
                    orderItem.UnitPriceAfterDiscount = unitPriceAfterDiscount;
                    orderItem.UnitDiscountAmount = unitDiscountAmount;
                    orderItem.LineTotal = lineTotal;
                    orderItem.Status = EntityStatusEnum.Active;
                    orderItem.InitializeEntity(userId.Value);
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
                        var (unitPriceOriginal, unitPriceAfterDiscount, unitDiscountAmount, lineTotal) = CalculateProductPricing(cartItem.Product, orderItem.Quantity);
                        orderItem.UnitPriceOriginal = unitPriceOriginal;
                        orderItem.UnitPriceAfterDiscount = unitPriceAfterDiscount;
                        orderItem.UnitDiscountAmount = unitDiscountAmount;
                        orderItem.LineTotal = lineTotal;
                        //update product stock
                        cartItem.Product.StockQuantity -= cartItem.Quantity;
                        _unitOfWork.Repository<Domain.Entities.Product>().Update(cartItem.Product);
                        orderItem.Status = EntityStatusEnum.Active;
                        orderItem.InitializeEntity(userId.Value);
                        newOrderItems.Add(orderItem);
                        productNames.Add(cartItem.Product.Name); // Store product name for metadata
                    }
                    _unitOfWork.Repository<Domain.Entities.CartItem>().DeleteRange(cartItems);
                }

                // ============================================
                // STEP 1: Tính subtotal và product discount
                // ============================================
                newOrder.SubtotalOriginal = newOrderItems.Sum(x => x.UnitPriceOriginal * x.Quantity);
                newOrder.ProductDiscountAmount = newOrderItems.Sum(x => x.UnitDiscountAmount * x.Quantity);
                newOrder.SubtotalAfterProductDiscount = newOrder.SubtotalOriginal - newOrder.ProductDiscountAmount;

                // ============================================
                // STEP 2: Validate và apply coupons (gộp validate rules và calculate discount)
                // ============================================
                if (request.UserCouponIds != null &&
                    request.UserCouponIds.Any())
                {
                    var distinctUserCouponIds = request.UserCouponIds.Distinct().ToList();
                    
                    // Load tất cả UserCoupons một lần với Coupon navigation property
                    var userCoupons = (await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
                        .FindAsync(
                            x => x.UserId == userId.Value && 
                                 distinctUserCouponIds.Contains(x.Id) && 
                                 x.UsedDate == null,
                            x => x.Coupon))
                        .ToList();

                    // Validate số lượng UserCoupons tìm được
                    if (userCoupons.Count != distinctUserCouponIds.Count)
                    {
                        var foundIds = userCoupons.Select(uc => uc.Id).ToHashSet();
                        var missingIds = distinctUserCouponIds.Where(id => !foundIds.Contains(id)).ToList();
                        throw new ArgumentException($"User coupon(s) not found, already used, or does not belong to user: {string.Join(", ", missingIds)}");
                    }

                    // Validate tất cả Coupons đã được load
                    var coupons = userCoupons.Select(uc => uc.Coupon).Where(c => c != null).Cast<Domain.Entities.Coupon>().ToList();
                    if (coupons.Count != userCoupons.Count)
                    {
                        throw new ArgumentException("Some coupons not found");
                    }

                    // Validate coupon rules (chỉ được 1 FreeShip, 1 Percentage/Fixed, không được dùng cả 2 cùng lúc)
                    var validationResult = ValidateCouponRules(coupons);
                    if (!validationResult.IsValid)
                    {
                        throw new ArgumentException(string.Join("; ", validationResult.Errors));
                    }

                    // Validate và tính discount cho tất cả coupons
                    // Note: FreeShipping coupons will be validated with MinOrderAmount on SubtotalAfterProductDiscount
                    var now = DateTime.UtcNow;
                    foreach (var userCoupon in userCoupons)
                    {
                        var coupon = userCoupon.Coupon!;
                        
                        // Kiểm tra coupon status
                        if (coupon.Status != EntityStatusEnum.Active)
                        {
                            throw new ArgumentException($"Coupon {coupon.Code} is not active");
                        }

                        // Kiểm tra thời gian hiệu lực
                        if (now < coupon.StartDate || now > coupon.EndDate)
                        {
                            throw new ArgumentException($"Mã {coupon.Code} đã hết hạn hoặc chưa đến thời gian sử dụng");
                        }

                        decimal discountAmount = 0m;
                        decimal baseAmount = 0m;

                        if (coupon.DiscountType == DiscountTypeEnum.FreeShipping)
                        {
                            // FreeShipping: Kiểm tra MinOrderAmount trên SubtotalAfterProductDiscount
                            if (newOrder.SubtotalAfterProductDiscount < coupon.MinOrderAmount)
                            {
                                throw new ArgumentException($"Minimum order amount of {coupon.MinOrderAmount:N0} is required for coupon {coupon.Code}");
                            }
                            
                            // baseAmount và discountAmount sẽ được set sau khi có shipping fee
                            baseAmount = 0m; // Will be updated when shipping is calculated
                            discountAmount = 0m; // Will be calculated based on shipping fee
                        }
                        else
                        {
                            // Kiểm tra minimum order amount (chỉ cho Percentage/Fixed)
                            if (newOrder.SubtotalAfterProductDiscount < coupon.MinOrderAmount)
                            {
                                throw new ArgumentException($"Minimum order amount of {coupon.MinOrderAmount:N0} is required for coupon {coupon.Code}");
                            }

                            baseAmount = newOrder.SubtotalAfterProductDiscount;

                            // Tính discount amount
                            if (coupon.DiscountType == DiscountTypeEnum.Percentage)
                            {
                                var calculated = newOrder.SubtotalAfterProductDiscount * (coupon.DiscountValue / 100);
                                discountAmount = coupon.MaxDiscountCap.HasValue 
                                    ? Math.Min(calculated, coupon.MaxDiscountCap.Value)
                                    : calculated;
                            }
                            else if (coupon.DiscountType == DiscountTypeEnum.Fixed)
                            {
                                discountAmount = Math.Min(coupon.DiscountValue, newOrder.SubtotalAfterProductDiscount);
                            }
                        }

                        appliedCoupons.Add((coupon, userCoupon, discountAmount, baseAmount));
                    }

                    // Apply product discount coupons (Percentage/Fixed)
                    var productDiscountCoupons = appliedCoupons
                        .Where(c => c.coupon.DiscountType != DiscountTypeEnum.FreeShipping)
                        .ToList();

                    decimal couponDiscountAmount = 0m;
                    foreach (var couponResult in productDiscountCoupons)
                    {
                        couponDiscountAmount += couponResult.discountAmount;
                    }

                    // Đảm bảo discount không vượt quá subtotal
                    couponDiscountAmount = Math.Min(couponDiscountAmount, newOrder.SubtotalAfterProductDiscount);
                    newOrder.CouponDiscountAmount = couponDiscountAmount;
                    newOrder.TotalProductAmount = newOrder.SubtotalAfterProductDiscount - newOrder.CouponDiscountAmount;
                }
                else
                {
                    newOrder.CouponDiscountAmount = 0m;
                    newOrder.TotalProductAmount = newOrder.SubtotalAfterProductDiscount;
                }

                newOrder.UserId = userId.Value;
                newOrder.InitializeEntity(userId.Value);

                // ============================================
                // STEP 3: Handle Shipping (if authenticated user)
                // ============================================
                if (userId.HasValue && request.ShippingMethodId.HasValue)
                {
                    var shippingMethod = await _unitOfWork.Repository<Domain.Entities.ShippingMethod>()
                        .GetFirstOrDefaultAsync(x => x.Id == request.ShippingMethodId.Value);
                    if (shippingMethod == null)
                        throw new KeyNotFoundException("Shipping method not found");

                    // Get shipping fee from request (client already calculated)
                    var shippingFeeOriginal = request.ShippingAmount ?? 0m;
                    
                    // Apply FreeShipping coupon discount if any
                    var freeShippingCoupons = appliedCoupons
                        .Where(c => c.coupon.DiscountType == DiscountTypeEnum.FreeShipping)
                        .ToList();
                    
                    decimal shippingDiscountAmount = 0m;
                    if (freeShippingCoupons.Any())
                    {
                        // Only apply first FreeShip coupon (validation already ensures only 1)
                        shippingDiscountAmount = shippingFeeOriginal; // 100% freeship
                    }
                    
                    var shippingFeeActual = Math.Max(0m, shippingFeeOriginal - shippingDiscountAmount);
                    
                    newOrder.ShippingFeeOriginal = shippingFeeOriginal;
                    newOrder.ShippingDiscountAmount = shippingDiscountAmount;
                    newOrder.ShippingFeeActual = shippingFeeActual;

                    // Create OrderShippingMethod record
                    orderShippingMethod = new Domain.Entities.OrderShippingMethod
                    {
                        OrderId = newOrder.Id,
                        ShippingMethodId = shippingMethod.Id,
                        ProviderName = shippingMethod.Name,
                        TrackingNumber = null, // Will be set after shipping order is created
                        ShippingFeeOriginal = shippingFeeOriginal,
                        ShippingDiscountAmount = shippingDiscountAmount,
                        ShippingFeeActual = shippingFeeActual,
                        IsFreeshipping = shippingDiscountAmount > 0,
                        FreeshippingNote = freeShippingCoupons.Any() 
                            ? $"Coupon {freeShippingCoupons.First().coupon.Code}" 
                            : null,
                        EstimatedDeliveryDate = request.EstimatedDeliveryDate,
                        Status = EntityStatusEnum.Active
                    };
                    orderShippingMethod.InitializeEntity(userId.Value);
                    await _unitOfWork.Repository<Domain.Entities.OrderShippingMethod>().AddAsync(orderShippingMethod);
                    
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
                    newOrder.ShippingFeeOriginal = 0m;
                    newOrder.ShippingDiscountAmount = 0m;
                    newOrder.ShippingFeeActual = 0m;
                }

                // ============================================
                // STEP 4: Tính tổng cuối
                // ============================================
                newOrder.FinalAmount = newOrder.TotalProductAmount + newOrder.ShippingFeeActual + newOrder.TaxAmount;
                
                // Validate FinalAmount before creating payment
                if (newOrder.FinalAmount <= 0)
                {
                    throw new ArgumentException("Cannot place order with zero or negative final amount. Please ensure cart has items or order has valid products.");
                }

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
                    metadata["SubtotalOriginal"] = newOrder.SubtotalOriginal.ToString("N0");
                    metadata["TotalProductAmount"] = newOrder.TotalProductAmount.ToString("N0");
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

                // Mark UserCoupons as used
                if (appliedCoupons.Any())
                {
                    MarkUserCouponsAsUsed(newOrder, userId.Value, appliedCoupons);
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

    private (decimal unitPriceOriginal, decimal unitPriceAfterDiscount, decimal unitDiscountAmount, decimal lineTotal) CalculateProductPricing(Domain.Entities.Product product, int quantity)
    {
        var unitPriceOriginal = product.Price;
        var unitPriceAfterDiscount = product.DiscountPrice.HasValue 
            && product.DiscountPrice.Value > 0  
            && product.DiscountPrice.Value <= product.Price
            ? product.DiscountPrice.Value 
            : product.Price;
        
        // tính toán giá trị giảm giá trên từng sản phẩm
        var unitDiscountAmount = unitPriceOriginal - unitPriceAfterDiscount;
        // tính toán giá trị giảm giá trên tổng sản phẩm
        var lineTotal = unitPriceAfterDiscount * quantity;

        return (unitPriceOriginal, unitPriceAfterDiscount, unitDiscountAmount, lineTotal);
    }

    /// <summary>
    /// Validate coupon rules (chỉ được 1 FreeShip, 1 Percentage/Fixed, không được dùng cả 2 cùng lúc)
    /// </summary>
    private (bool IsValid, List<string> Errors) ValidateCouponRules(List<Domain.Entities.Coupon> coupons)
    {
        var errors = new List<string>();

        // Đếm số lượng coupon theo loại
        var freeShipCount = coupons.Count(c => c.DiscountType == DiscountTypeEnum.FreeShipping);
        var percentageCount = coupons.Count(c => c.DiscountType == DiscountTypeEnum.Percentage);
        var fixedCount = coupons.Count(c => c.DiscountType == DiscountTypeEnum.Fixed);

        // RULE 1: Chỉ được 1 FreeShip coupon
        if (freeShipCount > 1)
        {
            errors.Add("Chỉ được áp dụng tối đa 1 mã miễn phí vận chuyển");
        }

        // RULE 2: Chỉ được chọn Percentage HOẶC Fixed (KHÔNG được dùng cả 2 cùng lúc)
        if (percentageCount > 0 && fixedCount > 0)
        {
            errors.Add("Chỉ được chọn mã giảm theo phần trăm HOẶC giảm cố định, không được dùng cả hai");
        }

        // RULE 3: Chỉ được 1 Percentage coupon
        if (percentageCount > 1)
        {
            errors.Add("Chỉ được áp dụng tối đa 1 mã giảm theo phần trăm");
        }

        // RULE 4: Chỉ được 1 Fixed coupon
        if (fixedCount > 1)
        {
            errors.Add("Chỉ được áp dụng tối đa 1 mã giảm cố định");
        }

        // RULE 5: Tổng số coupon không quá 2
        if (coupons.Count > 2)
        {
            errors.Add("Chỉ được áp dụng tối đa 2 mã giảm giá (1 FreeShip + 1 Percentage/Fixed)");
        }

        return (errors.Count == 0, errors);
    }


    /// <summary>
    /// Mark UserCoupons as used after order is placed
    /// </summary>
    private void MarkUserCouponsAsUsed(
        Domain.Entities.Order order,
        Guid? currentUserId,
        List<(Domain.Entities.Coupon coupon, Domain.Entities.UserCoupon userCoupon, decimal discountAmount, decimal baseAmount)> appliedCoupons)
    {
        foreach (var couponResult in appliedCoupons)
        {
            couponResult.userCoupon.OrderId = order.Id;
            couponResult.userCoupon.UsedDate = DateTime.UtcNow;
            couponResult.userCoupon.UpdateEntity(currentUserId!.Value);
            _unitOfWork.Repository<Domain.Entities.UserCoupon>().Update(couponResult.userCoupon);
        }
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
                Price = (int)item.UnitPriceAfterDiscount,
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

            // Update Order shipping fields if needed
            order.ShippingFeeOriginal = orderShippingMethod.ShippingFeeOriginal;
            order.ShippingDiscountAmount = orderShippingMethod.ShippingDiscountAmount;
            order.ShippingFeeActual = orderShippingMethod.ShippingFeeActual;
            order.FinalAmount = order.TotalProductAmount + order.ShippingFeeActual + order.TaxAmount;
            _unitOfWork.Repository<Domain.Entities.Order>().Update(order);

            return orderShippingMethod;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create shipping order after payment: {ex.Message}", ex);
        }
    }
}
