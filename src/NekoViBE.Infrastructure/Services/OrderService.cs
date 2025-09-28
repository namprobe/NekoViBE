using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers.Coupon;
using NekoViBE.Application.Common.Helpers.CreateOrder;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Infrastructure.Services
{

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult<Order>> CreateOrderAsync(CreateOrderRequest request, Guid? userId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validate products and stock
                var validationResult = await ValidateOrderItemsAsync(request.OrderItems, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return ServiceResult<Order>.Failure(validationResult.ErrorMessage, validationResult.ErrorCode);
                }

                // 2. Calculate order amounts
                var calculationResult = await CalculateOrderAmountsAsync(request.OrderItems, request.CouponCode, userId, cancellationToken);
                if (!calculationResult.IsSuccess)
                {
                    return ServiceResult<Order>.Failure(calculationResult.Message, calculationResult.ErrorCode);
                }

                // 3. Create order
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    UserId = userId,
                    IsOneClick = request.IsOneClick,
                    GuestEmail = request.GuestEmail,
                    GuestFirstName = request.GuestFirstName,
                    GuestLastName = request.GuestLastName,
                    GuestPhone = request.GuestPhone,
                    OneClickAddress = request.OneClickAddress,
                    TotalAmount = calculationResult.Data.TotalAmount,
                    DiscountAmount = calculationResult.Data.DiscountAmount,
                    TaxAmount = calculationResult.Data.TaxAmount,
                    ShippingAmount = calculationResult.Data.ShippingAmount,
                    FinalAmount = calculationResult.Data.FinalAmount,
                    PaymentStatus = PaymentStatusEnum.Pending,
                    OrderStatus = OrderStatusEnum.Processing,
                    Notes = request.Notes
                };
                order.InitializeEntity(userId);

                // 4. Create order items
                order.OrderItems = await CreateOrderItemsAsync(request.OrderItems, calculationResult.Data.ItemPrices, cancellationToken);

                // 5. Apply coupon if provided
                if (!string.IsNullOrEmpty(request.CouponCode))
                {
                    await ApplyCouponToOrderAsync(order, request.CouponCode, userId, cancellationToken);
                }

                // 6. Save order
                await _unitOfWork.Repository<Order>().AddAsync(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 7. Update product stock
                await UpdateProductStockAsync(request.OrderItems, cancellationToken);

                return ServiceResult<Order>.Success(order, "Order created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in order service");
                return ServiceResult<Order>.Failure("Error creating order", ErrorCodeEnum.InternalError);
            }
        }

        private async Task<ValidationResult> ValidateOrderItemsAsync(List<OrderItemRequest> orderItems, CancellationToken cancellationToken)
        {
            foreach (var item in orderItems)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    return ValidationResult.Failure($"Product {item.ProductId} not found", ErrorCodeEnum.NotFound);
                }

                if (product.StockQuantity < item.Quantity)
                {
                    return ValidationResult.Failure($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}", ErrorCodeEnum.ValidationFailed);
                }

                if (product.Status != EntityStatusEnum.Active)
                {
                    return ValidationResult.Failure($"Product {product.Name} is not available", ErrorCodeEnum.ValidationFailed);
                }

                // Check for pre-order items
                if (product.IsPreOrder && product.PreOrderReleaseDate > DateTime.UtcNow)
                {
                    return ValidationResult.Failure($"Product {product.Name} is available for pre-order only. Release date: {product.PreOrderReleaseDate:yyyy-MM-dd}", ErrorCodeEnum.ValidationFailed);
                }
            }

            return ValidationResult.Success();
        }

        private async Task<ServiceResult<OrderCalculation>> CalculateOrderAmountsAsync(
            List<OrderItemRequest> orderItems, string? couponCode, Guid? userId, CancellationToken cancellationToken)
        {
            decimal totalAmount = 0;
            decimal discountAmount = 0;
            var itemPrices = new Dictionary<Guid, decimal>();

            // Calculate item totals
            foreach (var item in orderItems)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                if (product == null) continue;

                //var unitPrice = product.DiscountPrice ?? product.Price;
                var unitPrice = product.Price; // For simplicity, ignoring product-level discounts here
                var itemTotal = unitPrice * item.Quantity;

                totalAmount += itemTotal;
                itemPrices[item.ProductId] = unitPrice;
            }

            // Apply coupon discount
            if (!string.IsNullOrEmpty(couponCode))
            {
                var couponResult = await ValidateAndCalculateCouponDiscountAsync(couponCode, totalAmount, userId, cancellationToken);
                if (couponResult.IsSuccess)
                {
                    discountAmount = couponResult.Data.DiscountAmount;
                }
                else
                {
                    return ServiceResult<OrderCalculation>.Failure(couponResult.Message, couponResult.ErrorCode);
                }
            }

            // Calculate tax (simplified - 8%)
            decimal taxAmount = (totalAmount - discountAmount) * 0.08m;

            // Calculate shipping (simplified - fixed $10)
            decimal shippingAmount = 10.00m;

            decimal finalAmount = (totalAmount - discountAmount) + taxAmount + shippingAmount;

            var calculation = new OrderCalculation
            {
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                TaxAmount = taxAmount,
                ShippingAmount = shippingAmount,
                FinalAmount = finalAmount,
                ItemPrices = itemPrices
            };

            return ServiceResult<OrderCalculation>.Success(calculation);
        }

        private async Task<List<OrderItem>> CreateOrderItemsAsync(
            List<OrderItemRequest> orderItems, Dictionary<Guid, decimal> itemPrices, CancellationToken cancellationToken)
        {
            var orderItemsList = new List<OrderItem>();

            foreach (var item in orderItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = itemPrices[item.ProductId],
                    DiscountAmount = 0 // Item-level discounts would be calculated here
                };
                orderItem.InitializeEntity();

                orderItemsList.Add(orderItem);
            }

            return orderItemsList;
        }

        private async Task UpdateProductStockAsync(List<OrderItemRequest> orderItems, CancellationToken cancellationToken)
        {
            foreach (var item in orderItems)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                if (product == null) continue;

                // Store original quantity for logging
                var originalQuantity = product.StockQuantity;

                // Update stock quantity
                product.StockQuantity -= item.Quantity;

                // Ensure stock doesn't go negative (safety check - should not happen due to validation)
                if (product.StockQuantity < 0)
                {
                    _logger.LogWarning("Product {ProductId} stock went negative (was {Original}, reduced by {Reduction})",
                        product.Id, originalQuantity, item.Quantity);
                    product.StockQuantity = 0;
                }

                // Update product
                product.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Product>().Update(product);

                _logger.LogInformation("Product {ProductId} stock updated: {Original} -> {New} (-{Reduction})",
                    product.Id, originalQuantity, product.StockQuantity, item.Quantity);

                // Create inventory tracking record
                await CreateInventoryRecordAsync(product, item.Quantity, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task CreateInventoryRecordAsync(Product product, int quantitySold, CancellationToken cancellationToken)
        {
            var inventoryRecord = new ProductInventory
            {
                ProductId = product.Id,
                Quantity = -quantitySold, // Negative for sales
             };
            inventoryRecord.InitializeEntity();

            await _unitOfWork.Repository<ProductInventory>().AddAsync(inventoryRecord);
        }

        private async Task<ServiceResult<CouponDiscountResult>> ValidateAndCalculateCouponDiscountAsync(
            string couponCode, decimal orderAmount, Guid? userId, CancellationToken cancellationToken)
        {
            // Get coupon by code
            var coupons = await _unitOfWork.Repository<Coupon>()
                .FindAsync(c => c.Code == couponCode && c.Status == EntityStatusEnum.Active);

            var coupon = coupons.FirstOrDefault();
            if (coupon == null)
            {
                return ServiceResult<CouponDiscountResult>.Failure("Invalid coupon code", ErrorCodeEnum.ValidationFailed);
            }

            // Validate coupon dates
            var now = DateTime.UtcNow;
            if (now < coupon.StartDate || now > coupon.EndDate)
            {
                return ServiceResult<CouponDiscountResult>.Failure("Coupon is not valid at this time", ErrorCodeEnum.ValidationFailed);
            }

            // Validate usage limit
            if (coupon.UsageLimit.HasValue && coupon.CurrentUsage >= coupon.UsageLimit.Value)
            {
                return ServiceResult<CouponDiscountResult>.Failure("Coupon usage limit exceeded", ErrorCodeEnum.ValidationFailed);
            }

            // Validate minimum order amount
            if (orderAmount < coupon.MinOrderAmount)
            {
                return ServiceResult<CouponDiscountResult>.Failure(
                    $"Minimum order amount of {coupon.MinOrderAmount:C} required for this coupon",
                    ErrorCodeEnum.ValidationFailed);
            }

            // Validate user-specific restrictions (if any)
            if (userId.HasValue)
            {
                var userCouponUsage = await _unitOfWork.Repository<UserCoupon>()
                    .FindAsync(uc => uc.UserId == userId && uc.CouponId == coupon.Id && uc.UsedDate != null);

                // Example: Limit one use per user
                if (userCouponUsage.Any())
                {
                    return ServiceResult<CouponDiscountResult>.Failure("You have already used this coupon", ErrorCodeEnum.ValidationFailed);
                }
            }

            // Calculate discount amount
            decimal discountAmount = CalculateDiscountAmount(coupon, orderAmount);

            var result = new CouponDiscountResult
            {
                Coupon = coupon,
                DiscountAmount = discountAmount,
                IsValid = true
            };

            return ServiceResult<CouponDiscountResult>.Success(result, "Coupon applied successfully");
        }

        private decimal CalculateDiscountAmount(Coupon coupon, decimal orderAmount)
        {
            return coupon.DiscountType switch
            {
                DiscountTypeEnum.Percentage => orderAmount * (coupon.DiscountValue / 100),
                DiscountTypeEnum.Fixed => coupon.DiscountValue,
                //DiscountTypeEnum.FreeShipping => 0, // This would be handled separately in shipping calculation
                _ => 0
            };
        }

        private async Task ApplyCouponToOrderAsync(Order order, string couponCode, Guid? userId, CancellationToken cancellationToken)
        {
            var coupons = await _unitOfWork.Repository<Coupon>()
                .FindAsync(c => c.Code == couponCode);

            var coupon = coupons.FirstOrDefault();
            if (coupon == null) return;

            // Create UserCoupon record
            var userCoupon = new UserCoupon
            {
                UserId = userId,
                CouponId = coupon.Id,
                OrderId = order.Id,
                UsedDate = DateTime.UtcNow
            };
            userCoupon.InitializeEntity(userId);

            await _unitOfWork.Repository<UserCoupon>().AddAsync(userCoupon);

            // Update coupon usage count
            coupon.CurrentUsage++;
            coupon.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Coupon>().Update(coupon);

            // Link coupon to order
            order.UserCoupons.Add(userCoupon);
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        public async Task<ServiceResult<bool>> CancelOrderAsync(Guid orderId, Guid? userId, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
                if (order == null)
                {
                    return ServiceResult<bool>.Failure("Order not found", ErrorCodeEnum.NotFound);
                }

                // Check if user owns the order or is admin
                if (order.UserId != userId && userId.HasValue)
                {
                    return ServiceResult<bool>.Failure("Access denied", ErrorCodeEnum.Forbidden);
                }

                // Check if order can be cancelled
                if (order.OrderStatus == OrderStatusEnum.Shipping || order.OrderStatus == OrderStatusEnum.Delivered)
                {
                    return ServiceResult<bool>.Failure("Cannot cancel order that has already been shipped", ErrorCodeEnum.ValidationFailed);
                }

                // Restore product stock
                await RestoreProductStockAsync(order, cancellationToken);

                // Update order status
                order.OrderStatus = OrderStatusEnum.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Order>().Update(order);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ServiceResult<bool>.Success(true, "Order cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                return ServiceResult<bool>.Failure("Error cancelling order", ErrorCodeEnum.InternalError);
            }
        }

        private async Task RestoreProductStockAsync(Order order, CancellationToken cancellationToken)
        {
            foreach (var orderItem in order.OrderItems)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(orderItem.ProductId);
                if (product == null) continue;

                // Restore stock
                product.StockQuantity += orderItem.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Product>().Update(product);

                // Create inventory record for restoration
                var inventoryRecord = new ProductInventory
                {
                    ProductId = product.Id,
                    Quantity = orderItem.Quantity, // Positive for restoration
                    
                };
                inventoryRecord.InitializeEntity();

                await _unitOfWork.Repository<ProductInventory>().AddAsync(inventoryRecord);
            }
        }
    }

}
