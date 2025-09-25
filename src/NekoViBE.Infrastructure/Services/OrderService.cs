using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers.CreateOrder;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using NekoViBE.Domain.Common;

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
                //if (!string.IsNullOrEmpty(request.CouponCode))
                //{
                //    await ApplyCouponToOrderAsync(order, request.CouponCode, userId, cancellationToken);
                //}

                // 6. Save order
                await _unitOfWork.Repository<Order>().AddAsync(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 7. Update product stock
                //await UpdateProductStockAsync(request.OrderItems, cancellationToken);

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
                    return ValidationResult.Failure($"Insufficient stock for product {product.Name}", ErrorCodeEnum.ValidationFailed);
                }

                if (product.Status != EntityStatusEnum.Active)
                {
                    return ValidationResult.Failure($"Product {product.Name} is not available", ErrorCodeEnum.ValidationFailed);
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
                var unitPrice = product.Price;

                var itemTotal = unitPrice * item.Quantity;

                totalAmount += itemTotal;
                itemPrices[item.ProductId] = unitPrice;
            }

            //Apply coupon discount
            //if (!string.IsNullOrEmpty(couponCode))
            //{
            //    var couponResult = await ValidateAndCalculateCouponDiscountAsync(couponCode, totalAmount, userId, cancellationToken);
            //    if (couponResult.IsSuccess)
            //    {
            //        discountAmount = couponResult.Data.DiscountAmount;
            //    }
            //    else
            //    {
            //        return ServiceResult<OrderCalculation>.Failure(couponResult.Message, couponResult.ErrorCode);
            //    }
            //}

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

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        // ... other helper methods for coupon validation, stock update, etc.
    }
}
