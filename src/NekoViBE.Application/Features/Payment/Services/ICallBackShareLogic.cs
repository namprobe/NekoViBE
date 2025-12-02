using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Payment.Services;

/// <summary>
/// Service để rollback các thay đổi khi order fail: restore stock và revert coupon
/// </summary>
public interface ICallBackShareLogic
{
    /// <summary>
    /// Revert các thay đổi khi order fail: restore stock và revert coupon
    /// </summary>
    /// <param name="order">Order cần revert</param>
    /// <param name="unitOfWork">Unit of work instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task RevertOrderChangesAsync(
        Domain.Entities.Order order, 
        IUnitOfWork unitOfWork, 
        ILogger logger, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Update order thành failed
    /// </summary>
    /// <param name="order">Order cần update</param>
    /// <param name="note">Note message</param>
    /// <param name="unitOfWork">Unit of work instance</param>
    void UpdateOrderAsFailed(Domain.Entities.Order order, string note, IUnitOfWork unitOfWork);

    /// <summary>
    /// Update payment thành failed
    /// </summary>
    /// <param name="payment">Payment cần update</param>
    /// <param name="note">Note message</param>
    /// <param name="processorResponse">Processor response message</param>
    /// <param name="unitOfWork">Unit of work instance</param>
    void UpdatePaymentAsFailed(
        Domain.Entities.Payment payment, 
        string note, 
        string processorResponse, 
        IUnitOfWork unitOfWork);

    /// <summary>
    /// Rollback shipping order if it was created (cancel shipping order)
    /// </summary>
    /// <param name="order">Order cần rollback shipping</param>
    /// <param name="unitOfWork">Unit of work instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackShippingOrderAsync(
        Domain.Entities.Order order,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create shipping order after payment success
    /// </summary>
    /// <param name="order">Order đã thanh toán thành công</param>
    /// <param name="shippingService">Shipping service instance (GHN, GHTK, etc.)</param>
    /// <param name="unitOfWork">Unit of work instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateShippingOrderAfterPaymentSuccessAsync(
        Domain.Entities.Order order,
        IShippingService shippingService,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken cancellationToken);
}

