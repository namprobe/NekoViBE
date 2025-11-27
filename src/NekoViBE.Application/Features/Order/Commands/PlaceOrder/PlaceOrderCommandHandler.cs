using System;
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
    private readonly IServiceProvider _serviceProvider;

    public PlaceOrderCommandHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPaymentGatewayFactory paymentGatewayFactory,
        IServiceProvider serviceProvider)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _paymentGatewayFactory = paymentGatewayFactory;
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
            Domain.Entities.Coupon? appliedCoupon = null;
            Domain.Entities.UserCoupon? appliedUserCoupon = null;
            string? paymentUrl = null;
            PaymentGatewayType? paymentGatewayType = null;

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

                newOrder.OrderItems = newOrderItems;
                newOrder.TotalAmount = newOrderItems.Sum(x => x.UnitPrice * x.Quantity);

                var amountAfterProductDiscount = newOrder.TotalAmount - productDiscountAmount;
                if (request.UserCouponId.HasValue && amountAfterProductDiscount > 0)
                {
                    var couponResult = await ValidateUserCouponAsync(request.UserCouponId.Value, amountAfterProductDiscount, userId.Value, cancellationToken);
                    couponDiscountAmount = couponResult.discountAmount;
                    appliedCoupon = couponResult.coupon;
                    appliedUserCoupon = couponResult.userCoupon;
                }

                newOrder.DiscountAmount = productDiscountAmount + couponDiscountAmount;
                newOrder.FinalAmount = Math.Max(0, newOrder.TotalAmount - newOrder.DiscountAmount);
                newOrder.UserId = userId.Value;
                newOrder.InitializeEntity(userId.Value);
                await _unitOfWork.Repository<Domain.Entities.Order>().AddAsync(newOrder);

                //2. init payment
                var payment = new Domain.Entities.Payment();
                payment.OrderId = newOrder.Id;
                payment.PaymentMethodId = paymentMethod.Id;
                payment.Amount = newOrder.FinalAmount;
                payment.Status = EntityStatusEnum.Active;
                payment.InitializeEntity(userId.Value);
                await _unitOfWork.Repository<Domain.Entities.Payment>().AddAsync(payment);

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

                if (appliedCoupon is not null && appliedUserCoupon is not null)
                {
                    await AttachCouponToOrderAsync(newOrder, appliedCoupon, appliedUserCoupon, userId.Value);
                }

                //3. To do: Init Shipping
                //var shipping = new Domain.Entities.OrderShippingMethod();

                //await _unitOfWork.Repository<Domain.Entities.OrderShippingMethod>().AddRangeAsync(shipping);
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

    private Task AttachCouponToOrderAsync(
        Domain.Entities.Order order,
        Domain.Entities.Coupon coupon,
        Domain.Entities.UserCoupon userCoupon,
        Guid userId)
    {
        // Update UserCoupon đã tồn tại (không tạo mới)
        userCoupon.OrderId = order.Id;
        userCoupon.UsedDate = DateTime.UtcNow;
        userCoupon.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Domain.Entities.UserCoupon>().Update(userCoupon);

        // Thêm vào order navigation property
        if (order.UserCoupons == null)
        {
            order.UserCoupons = new List<Domain.Entities.UserCoupon>();
        }
        order.UserCoupons.Add(userCoupon);

        // Update coupon usage count
        coupon.CurrentUsage++;
        coupon.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Domain.Entities.Coupon>().Update(coupon);

        return Task.CompletedTask;
    }
}
