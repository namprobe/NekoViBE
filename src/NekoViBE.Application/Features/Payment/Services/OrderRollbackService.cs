using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Domain.Enums;
using System.Linq;

namespace NekoViBE.Application.Features.Payment.Services;

/// <summary>
/// Service để rollback các thay đổi khi order fail: restore stock và revert coupon
/// </summary>
public class OrderRollbackService : IOrderRollbackService
{
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
            if (order.UserCoupons == null || !order.UserCoupons.Any())
            {
                var userCoupons = await unitOfWork.Repository<Domain.Entities.UserCoupon>()
                    .FindAsync(x => x.OrderId == order.Id && x.UsedDate != null, x => x.Coupon);
                order.UserCoupons = userCoupons.ToList();
            }

            foreach (var userCoupon in order.UserCoupons.Where(uc => uc.UsedDate != null))
            {
                if (userCoupon.Coupon == null)
                {
                    userCoupon.Coupon = await unitOfWork.Repository<Domain.Entities.Coupon>()
                        .GetFirstOrDefaultAsync(x => x.Id == userCoupon.CouponId);
                }

                if (userCoupon.Coupon != null && userCoupon.Coupon.CurrentUsage > 0)
                {
                    userCoupon.Coupon.CurrentUsage--;
                    userCoupon.Coupon.UpdatedAt = DateTime.UtcNow;
                    unitOfWork.Repository<Domain.Entities.Coupon>().Update(userCoupon.Coupon);
                    logger.LogInformation(
                        "[Order Rollback] Reverted coupon usage for coupon {CouponId}: CurrentUsage = {CurrentUsage} (OrderId: {OrderId})",
                        userCoupon.CouponId, userCoupon.Coupon.CurrentUsage, order.Id);
                }

                // Revert UserCoupon
                userCoupon.UsedDate = null;
                userCoupon.OrderId = null;
                userCoupon.UpdatedAt = DateTime.UtcNow;
                unitOfWork.Repository<Domain.Entities.UserCoupon>().Update(userCoupon);
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
}

