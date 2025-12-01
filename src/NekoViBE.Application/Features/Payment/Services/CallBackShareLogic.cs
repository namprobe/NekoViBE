using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;
using System.Linq;

namespace NekoViBE.Application.Features.Payment.Services;

/// <summary>
/// Service để rollback các thay đổi khi order fail: restore stock và revert coupon
/// </summary>
public class CallBackShareLogic : ICallBackShareLogic
{
    private readonly IShippingServiceFactory? _shippingServiceFactory;

    public CallBackShareLogic(IShippingServiceFactory? shippingServiceFactory = null)
    {
        _shippingServiceFactory = shippingServiceFactory;
    }

    /// <summary>
    /// Revert các thay đổi khi order fail: restore stock và revert coupon
    /// </summary>
    public async Task RevertOrderChangesAsync(
        Domain.Entities.Order order,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        if (unitOfWork == null)
            throw new ArgumentNullException(nameof(unitOfWork));
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        try
        {
            // Load order items với product nếu chưa được load
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                var orderItems = await unitOfWork.Repository<Domain.Entities.OrderItem>()
                    .FindAsync(x => x.OrderId == order.Id, x => x.Product);
                order.OrderItems = orderItems.ToList();
            }

            // Restore stock cho tất cả order items
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.Product == null)
                {
                    orderItem.Product = await unitOfWork.Repository<Domain.Entities.Product>()
                        .GetFirstOrDefaultAsync(x => x.Id == orderItem.ProductId);
                }

                if (orderItem.Product != null)
                {
                    orderItem.Product.StockQuantity += orderItem.Quantity;
                    orderItem.Product.UpdatedAt = DateTime.UtcNow;
                    unitOfWork.Repository<Domain.Entities.Product>().Update(orderItem.Product);
                    logger.LogInformation(
                        "[Order Rollback] Restored stock for product {ProductId}: +{Quantity} (OrderId: {OrderId})",
                        orderItem.ProductId, orderItem.Quantity, order.Id);
                }
            }

            // Revert coupon usage nếu có
            var userCoupons = await unitOfWork.Repository<Domain.Entities.UserCoupon>()
                .FindAsync(x => x.OrderId == order.Id && x.UsedDate != null, x => x.Coupon);

            foreach (var userCoupon in userCoupons.Where(uc => uc.UsedDate != null))
            {
                // Revert UserCoupon
                userCoupon.UsedDate = null;
                userCoupon.OrderId = null;
                // Use UpdateEntity with userId from order (system update if userId is null)
                userCoupon.UpdateEntity(order.UserId ?? Guid.Empty);
                unitOfWork.Repository<Domain.Entities.UserCoupon>().Update(userCoupon);
                
                logger.LogInformation(
                    "[Order Rollback] Reverted UserCoupon {UserCouponId} for OrderId {OrderId}",
                    userCoupon.Id, order.Id);
            }

            logger.LogInformation("[Order Rollback] Successfully reverted order changes for OrderId: {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Order Rollback] Error reverting order changes for OrderId: {OrderId}", order.Id);
            // Không throw exception để không ảnh hưởng đến flow chính
        }
    }

    /// <summary>
    /// Update order thành failed
    /// </summary>
    public void UpdateOrderAsFailed(Domain.Entities.Order order, string note, IUnitOfWork unitOfWork)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        if (unitOfWork == null)
            throw new ArgumentNullException(nameof(unitOfWork));

        order.OrderStatus = OrderStatusEnum.Failed;
        order.PaymentStatus = PaymentStatusEnum.Failed;
        order.Notes = note;
        order.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Repository<Domain.Entities.Order>().Update(order);
    }

    /// <summary>
    /// Update payment thành failed
    /// </summary>
    public void UpdatePaymentAsFailed(
        Domain.Entities.Payment payment,
        string note,
        string processorResponse,
        IUnitOfWork unitOfWork)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));
        if (unitOfWork == null)
            throw new ArgumentNullException(nameof(unitOfWork));

        payment.PaymentStatus = PaymentStatusEnum.Failed;
        payment.Notes = note;
        payment.ProcessorResponse = processorResponse;
        payment.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Repository<Domain.Entities.Payment>().Update(payment);
    }

    /// <summary>
    /// Rollback shipping order if it was created (cancel shipping order)
    /// </summary>
    public async Task RollbackShippingOrderAsync(
        Domain.Entities.Order order,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        if (unitOfWork == null)
            throw new ArgumentNullException(nameof(unitOfWork));
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        try
        {
            // Load order shipping methods
            var orderShippingMethods = await unitOfWork.Repository<Domain.Entities.OrderShippingMethod>()
                .FindAsync(x => x.OrderId == order.Id, x => x.ShippingMethod!);

            foreach (var orderShippingMethod in orderShippingMethods)
            {
                // Only cancel if tracking number exists (shipping order was created)
                if (string.IsNullOrWhiteSpace(orderShippingMethod.TrackingNumber))
                {
                    logger.LogInformation(
                        "[Order Rollback] Shipping order not created yet for OrderShippingMethod {Id}, skipping cancel",
                        orderShippingMethod.Id);
                    continue;
                }

                var shippingMethod = orderShippingMethod.ShippingMethod;
                if (shippingMethod == null)
                {
                    continue;
                }

                // Check if shipping method is GHN
                if (!Enum.TryParse<ShippingProviderType>(shippingMethod.Name, out var providerType) ||
                    providerType != ShippingProviderType.GHN)
                {
                    logger.LogInformation(
                        "[Order Rollback] Shipping method is not GHN for OrderShippingMethod {Id}, skipping cancel",
                        orderShippingMethod.Id);
                    continue;
                }

                // Cancel shipping order via GHN API
                try
                {
                    if (_shippingServiceFactory == null)
                    {
                        logger.LogWarning(
                            "[Order Rollback] IShippingServiceFactory not available, cannot cancel shipping order {TrackingNumber}",
                            orderShippingMethod.TrackingNumber);
                        // Clear tracking number to indicate it should be canceled
                        orderShippingMethod.TrackingNumber = null;
                        orderShippingMethod.UpdatedAt = DateTime.UtcNow;
                        unitOfWork.Repository<Domain.Entities.OrderShippingMethod>().Update(orderShippingMethod);
                        continue;
                    }

                    var ghnService = _shippingServiceFactory.GetShippingService(ShippingProviderType.GHN);
                    var cancelResult = await ghnService.CancelOrderAsync(orderShippingMethod.TrackingNumber);

                    if (cancelResult.IsSuccess)
                    {
                        logger.LogInformation(
                            "[Order Rollback] Successfully canceled shipping order {TrackingNumber} for OrderId {OrderId}",
                            orderShippingMethod.TrackingNumber, order.Id);

                        // Clear tracking number to indicate shipping order was canceled
                        orderShippingMethod.TrackingNumber = null;
                        orderShippingMethod.UpdatedAt = DateTime.UtcNow;
                        unitOfWork.Repository<Domain.Entities.OrderShippingMethod>().Update(orderShippingMethod);
                    }
                    else
                    {
                        logger.LogWarning(
                            "[Order Rollback] Failed to cancel shipping order {TrackingNumber} for OrderId {OrderId}: {Message}",
                            orderShippingMethod.TrackingNumber, order.Id, cancelResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "[Order Rollback] Error canceling shipping order {TrackingNumber} for OrderId {OrderId}",
                        orderShippingMethod.TrackingNumber, order.Id);
                    // Don't throw - continue with other rollback operations
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Order Rollback] Error rolling back shipping orders for OrderId {OrderId}", order.Id);
            // Don't throw exception để không ảnh hưởng đến flow chính
        }
    }

    //     private long CalculateGHNPickupTime()
    //     {
    //     var now = DateTimeOffset.UtcNow;
    //     var today = now.Date.AddHours(7); // Convert UTC -> ICT (+7)

    //     // Nếu hiện tại > 17h hoặc cuối tuần (Sat=6, Sun=0), nhảy sang thứ 2 tuần sau
    //     if (today.DayOfWeek == DayOfWeek.Saturday || 
    //         today.DayOfWeek == DayOfWeek.Sunday || 
    //         today.Hour >= 17)
    //     {
    //         // Tìm thứ 2 tuần sau (thêm 2-3 ngày)
    //         var daysToMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
    //         if (daysToMonday == 0) daysToMonday = 7; // Nếu hôm nay CN thì +1
    //         var monday = today.AddDays(daysToMonday).Date.AddHours(10); // 10h sáng thứ 2
    //         return new DateTimeOffset(monday).ToUnixTimeSeconds();
    //     }

    //     // Hôm nay T2-T6:
    //     // Nếu trước 17h -> hôm nay 16h (để shipper kịp)
    //     // Nếu sau 17h -> mai 10h
    //     var pickupDate = today.Hour < 17 
    //         ? today.Date.AddHours(16)  // Hôm nay 16h
    //         : today.AddDays(1).Date.AddHours(10); // Mai 10h

    //     return new DateTimeOffset(pickupDate).ToUnixTimeSeconds();
    // }

    /// <summary>
    /// Create shipping order after payment success
    /// </summary>
    public async Task CreateShippingOrderAfterPaymentSuccessAsync(
        Domain.Entities.Order order,
        IShippingService shippingService,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        if (shippingService == null)
            throw new ArgumentNullException(nameof(shippingService));
        if (unitOfWork == null)
            throw new ArgumentNullException(nameof(unitOfWork));
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        try
        {
            // Load order shipping method
            var orderShippingMethods = await unitOfWork.Repository<Domain.Entities.OrderShippingMethod>()
                .FindAsync(x => x.OrderId == order.Id, x => x.ShippingMethod!);

            var orderShippingMethod = orderShippingMethods.FirstOrDefault();
            if (orderShippingMethod == null)
            {
                logger.LogInformation("[Shipping] No shipping method found for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }

            // If tracking number already exists, shipping order was already created
            if (!string.IsNullOrWhiteSpace(orderShippingMethod.TrackingNumber))
            {
                logger.LogInformation("[Shipping] Shipping order already created for order {OrderId} with tracking {TrackingNumber}",
                    order.Id, orderShippingMethod.TrackingNumber);
                return;
            }

            var shippingMethod = orderShippingMethod.ShippingMethod;
            if (shippingMethod == null)
            {
                logger.LogWarning("[Shipping] Shipping method not found for OrderShippingMethod {Id}", orderShippingMethod.Id);
                return;
            }

            // Check if shipping method matches the service provider
            if (!Enum.TryParse<ShippingProviderType>(shippingMethod.Name, out var providerType))
            {
                logger.LogInformation("[Shipping] Cannot parse shipping provider type for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }

            // Get user address
            var userAddress = await unitOfWork.Repository<Domain.Entities.UserAddress>()
                .GetFirstOrDefaultAsync(
                    x => x.UserId == order.UserId && x.IsDefault);

            if (userAddress == null)
            {
                logger.LogWarning("[Shipping] User address not found for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }

            // Validate required fields for GHN API
            if (string.IsNullOrWhiteSpace(userAddress.FullName))
            {
                logger.LogWarning("[Shipping] User address FullName is missing for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }
            if (string.IsNullOrWhiteSpace(userAddress.PhoneNumber))
            {
                logger.LogWarning("[Shipping] User address PhoneNumber is missing for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }
            if (string.IsNullOrWhiteSpace(userAddress.Address))
            {
                logger.LogWarning("[Shipping] User address Address is missing for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }
            if (string.IsNullOrWhiteSpace(userAddress.WardName))
            {
                logger.LogWarning("[Shipping] User address WardName is missing for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }
            if (string.IsNullOrWhiteSpace(userAddress.DistrictName))
            {
                logger.LogWarning("[Shipping] User address DistrictName is missing for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }
            if (string.IsNullOrWhiteSpace(userAddress.ProvinceName))
            {
                logger.LogWarning("[Shipping] User address ProvinceName is missing for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }

            // Load order items
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                var orderItems = await unitOfWork.Repository<Domain.Entities.OrderItem>()
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

            // Get payment method
            var payment = await unitOfWork.Repository<Domain.Entities.Payment>()
                .GetFirstOrDefaultAsync(x => x.OrderId == order.Id, x => x.PaymentMethod!);

            var paymentMethod = payment?.PaymentMethod;
            if (paymentMethod == null)
            {
                logger.LogWarning("[Shipping] Payment method not found for order {OrderId}, skipping shipping order creation", order.Id);
                return;
            }

            // Set pickup_time to tomorrow (Unix timestamp in seconds)
            // GHN requires a valid future timestamp for request_delivery_time calculation
            // Use tomorrow to ensure it's in the future
            //var pickupTime = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            //var pickupTime = CalculateGHNPickupTime();

            // var shippingOrderRequest = new ShippingOrderRequest
            // {
            //     // ClientOrderCode = order.Id.ToString(),
            //     ClientOrderCode = "",
            //     ToName = userAddress.FullName,
            //     ToPhone = userAddress.PhoneNumber, // Already validated above
            //     ToAddress = userAddress.Address, // Already validated above
            //     ToWardName = userAddress.WardName, // Already validated above
            //     ToDistrictName = userAddress.DistrictName, // Already validated above
            //     ToProvinceName = userAddress.ProvinceName, // Already validated above
            //     PaymentTypeId = paymentMethod.IsOnlinePayment ? 2 : 1,
            //     ServiceTypeId = 2,
            //     RequiredNote = "KHONGCHOXEMHANG",
            //     Note = order.Notes,
            //     Weight = 500,
            //     Length = 20,
            //     Width = 20,
            //     Height = 20,
            //     CodAmount = !paymentMethod.IsOnlinePayment ? (int)order.FinalAmount : null,
            //     InsuranceValue = (int)order.FinalAmount,
            //     PickupTime = pickupTime, // Set valid future timestamp
            //     PickShift = new List<int> { 2 }, // Default pick shift [2] as per GHN example
            //     Items = shippingItems // Send items if available (GHN accepts items for service_type_id = 2)
            // };  

            //hard code để test theo curl thành công
            var shippingOrderRequest = new ShippingOrderRequest
            {
                // === EXACT CURL SAMPLE VALUES ===
                PaymentTypeId = 2,
                Note = "Test order", // Giữ ngắn gọn như curl
                RequiredNote = "KHONGCHOXEMHANG",
                ReturnPhone = "0332190158",
                ReturnAddress = "39 NTT",
                ReturnDistrictId = null,
                ReturnWardCode = "",
                ClientOrderCode = order.Id.ToString(), // Dùng order ID thực
                FromName = "NekoVi Store",
                FromPhone = "0867619150",
                FromAddress = "Lô E2a-1, E2a-2, đường D1, Đ. D1, Long Thạnh Mỹ, Quận 9, Hồ Chí Minh, Vietnam",
                FromWardName = "Long Thạnh Mỹ",
                FromDistrictName = "Quận 9",
                FromProvinceName = "Hồ Chí Minh",

                // Customer address (override validation tạm)
                ToName = userAddress.FullName,
                ToPhone = userAddress.PhoneNumber,
                ToAddress = userAddress.Address,
                ToWardName = userAddress.WardName,
                ToDistrictName = userAddress.DistrictName,
                ToProvinceName = userAddress.ProvinceName,

                CodAmount = 200000, // Hardcode như curl
                Content = "Test order content", // Giữ ngắn
                Length = 12, // Curl values
                Width = 12,
                Height = 12,
                Weight = 1200,
                CodFailedAmount = 2000,
                PickStationId = 1444, // Curl value
                DeliverStationId = null,
                InsuranceValue = 200000,
                ServiceTypeId = 2,
                Coupon = null,
                PickupTime = 1692840132L, // EXACT curl timestamp (works!)
                PickShift = new List<int> { 2 },

                // Items EXACT như curl (service_type_id=2 vẫn OK)
                Items = new List<ShippingOrderItem>
    {
        new ShippingOrderItem
        {
            Name = "Áo Polo",
            Code = "Polo123",
            Quantity = 1,
            Price = 200000,
            Length = 12,
            Width = 12,
            Height = 12,
            Weight = 1200,
            Category = new ShippingOrderItemCategory { Level1 = "Áo" }
        }
    }
            };


            // Preview order
            // var previewResult = await shippingService.PreviewOrderAsync(shippingOrderRequest);
            // if (!previewResult.IsSuccess)
            // {
            //     logger.LogError("[Shipping] Failed to preview shipping order for order {OrderId}: {Message}", 
            //         order.Id, previewResult.Message);
            //     return;
            // }

            // Create shipping order
            var createResult = await shippingService.CreateOrderAsync(shippingOrderRequest);
            if (!createResult.IsSuccess || createResult.Data == null)
            {
                logger.LogError("[Shipping] Failed to create shipping order for order {OrderId}: {Message}",
                    order.Id, createResult.Message);
                return;
            }

            // Update OrderShippingMethod with tracking number
            // NOTE: Do NOT call SaveChangesAsync here - let the caller manage the transaction
            orderShippingMethod.TrackingNumber = createResult.Data.OrderCode;
            orderShippingMethod.EstimatedDeliveryDate =
                createResult.Data.EstimatedDeliveryDate ?? createResult.Data.ExpectedDeliveryTime;
            orderShippingMethod.UpdatedAt = DateTime.UtcNow;
            unitOfWork.Repository<Domain.Entities.OrderShippingMethod>().Update(orderShippingMethod);

            // Create shipping history record for initial order creation
            // When order is created successfully, default status is "ready_to_pick"
            var (statusCode, statusName, statusDescription) = ShippingStatusHelper.MapGHNStatus("ready_to_pick");
            
            var shippingHistory = new Domain.Entities.ShippingHistory
            {
                OrderShippingMethodId = orderShippingMethod.Id,
                OrderId = order.Id,
                TrackingNumber = createResult.Data.OrderCode,
                StatusCode = statusCode,
                StatusName = statusName,
                StatusDescription = statusDescription,
                EventType = "order_created", // Custom event type for initial creation
                EventTime = DateTime.UtcNow,
                AdditionalData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    OrderCode = createResult.Data.OrderCode,
                    TotalFee = createResult.Data.TotalFee,
                    ExpectedDeliveryTime = createResult.Data.ExpectedDeliveryTime,
                    EstimatedDeliveryDate = createResult.Data.EstimatedDeliveryDate
                })
            };
            shippingHistory.InitializeEntity(Guid.Empty); // System update
            await unitOfWork.Repository<Domain.Entities.ShippingHistory>().AddAsync(shippingHistory);

            // Removed SaveChangesAsync - caller should manage transaction and save changes

            logger.LogInformation("[Shipping] Shipping order created successfully for order {OrderId} with tracking {TrackingNumber}",
                order.Id, createResult.Data.OrderCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Shipping] Error creating shipping order after payment success for order {OrderId}", order.Id);
            // Don't throw - shipping order can be created manually later
        }
    }
}

