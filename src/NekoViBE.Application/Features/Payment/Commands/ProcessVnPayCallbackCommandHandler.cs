using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Helpers.PaymentHelper;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Payment.Services;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Payment.Commands;

public class ProcessVnPayCallbackCommandHandler : IRequestHandler<ProcessVnPayCallbackCommand, Result<object>>
{
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessVnPayCallbackCommandHandler> _logger;
    private readonly ICallBackShareLogic _callBackShareLogic;
    private readonly IShippingServiceFactory _shippingServiceFactory;
    private readonly IServiceProvider _serviceProvider;

    public ProcessVnPayCallbackCommandHandler(
        IPaymentGatewayFactory paymentGatewayFactory, 
        IUnitOfWork unitOfWork, 
        ILogger<ProcessVnPayCallbackCommandHandler> logger,
        ICallBackShareLogic callBackShareLogic,
        IShippingServiceFactory shippingServiceFactory,
        IServiceProvider serviceProvider)
    {
        _paymentGatewayFactory = paymentGatewayFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _callBackShareLogic = callBackShareLogic;
        _shippingServiceFactory = shippingServiceFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<object>> Handle(ProcessVnPayCallbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var paymentGateway = _paymentGatewayFactory.GetPaymentGatewayService(PaymentGatewayType.VnPay);
            var paymentResult = paymentGateway.VerifyIpnRequest(request.QueryParams);
            
            // Lấy thông tin transaction
            var transaction = paymentResult.Data as VnPayTransactionResponse;
            
            // Generate note message từ payment result
            var paymentNote = PaymentNoteHelper.GeneratePaymentNote(paymentResult, transaction);
            
            // Kiểm tra payment result: nếu fail, update order và payment (nếu có) thành failed
            if (!paymentResult.IsSuccess)
            {
                if (transaction != null)
                {
                    var failedOrderId = Guid.Parse(transaction.TnxRef);
                    // QUAN TRỌNG: chỉ update order status thành failed nếu order status là Processing
                    var failedOrder = await _unitOfWork.Repository<Domain.Entities.Order>()
                        .GetFirstOrDefaultAsync(x => x.Id == failedOrderId && x.OrderStatus == OrderStatusEnum.Processing, 
                            o => o.Payment, 
                            o => o.OrderItems);
                    
                    if (failedOrder != null)
                    {
                        // Revert stock và coupon trước khi update order status
                        await _callBackShareLogic.RevertOrderChangesAsync(
                            failedOrder, _unitOfWork, _logger, cancellationToken);
                        
                        // Rollback shipping order if it was created
                        await _callBackShareLogic.RollbackShippingOrderAsync(
                            failedOrder, _unitOfWork, _logger, cancellationToken);
                        
                        _callBackShareLogic.UpdateOrderAsFailed(failedOrder, _unitOfWork);
                        
                        if (failedOrder.Payment != null)
                        {
                            _callBackShareLogic.UpdatePaymentAsFailed(
                                failedOrder.Payment, paymentNote, paymentResult.Message, _unitOfWork);
            }
                        
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        LogOrderAction(
                            failedOrder,
                            "[VNPay IPN] Order marked as failed",
                            new { failedOrder.OrderStatus, failedOrder.PaymentStatus, paymentNote },
                            cancellationToken);

                        if (failedOrder.Payment != null)
                        {
                            LogPaymentAction(
                                failedOrder,
                                failedOrder.Payment,
                                "[VNPay IPN] Payment marked as failed",
                                new { failedOrder.Payment.PaymentStatus, failedOrder.Payment.ProcessorResponse },
                                cancellationToken);
                        }
                    }
                }
                
                throw new Exception(paymentResult.Message);
            }
            
            // Payment result success: kiểm tra transaction null
            if (transaction == null)
            {
                throw new Exception("Transaction response is null but payment result is success");
            }
            
            // Kiểm tra order
            var orderId = Guid.Parse(transaction.TnxRef);
            var order = await _unitOfWork.Repository<Domain.Entities.Order>()
                .GetFirstOrDefaultAsync(x => x.Id == orderId && x.OrderStatus == OrderStatusEnum.Processing, o => o.Payment);
            // QUAN TRỌNG: chỉ update order status thành failed hoặc confirmed nếu order status là Processing
            if (order == null)
            {
                throw new Exception($"Order not found for TnxRef: {transaction.TnxRef} (OrderId: {orderId})");
            }
            
            // Kiểm tra payment
            var payment = order.Payment;
            
            if (payment == null)
            {
                // Revert stock và coupon trước khi update order status
                await _callBackShareLogic.RevertOrderChangesAsync(
                    order, _unitOfWork, _logger, cancellationToken);
                
                // Rollback shipping order if it was created
                await _callBackShareLogic.RollbackShippingOrderAsync(
                    order, _unitOfWork, _logger, cancellationToken);
                
                // Update order fail và save changes trước khi throw
                _callBackShareLogic.UpdateOrderAsFailed(
                    order, _unitOfWork);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                LogOrderAction(
                    order,
                    "[VNPay IPN] Order reverted due to missing payment record",
                    new { order.OrderStatus, order.PaymentStatus, paymentNote },
                    cancellationToken);
                
                throw new Exception($"Payment not found for Order: {order.Id} (TnxRef: {transaction.TnxRef})");
            }
            
            // Cập nhật trạng thái order và payment thành công
            order.OrderStatus = OrderStatusEnum.Confirmed;
            order.PaymentStatus = PaymentStatusEnum.Completed;
            order.Notes = "Order Confirmed";
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Domain.Entities.Order>().Update(order);
            
            payment.TransactionNo = transaction.TransactionNo;
            payment.PaymentStatus = PaymentStatusEnum.Completed;
            payment.PaymentDate = transaction.PaymentDate;
            payment.Notes = paymentNote;
            payment.ProcessorResponse = paymentResult.Message;
            payment.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Domain.Entities.Payment>().Update(payment);
            
            // Create shipping order after payment success (before saving changes)
            // This ensures everything is in the same transaction
            try
            {
                var orderShippingMethods = await _unitOfWork.Repository<Domain.Entities.OrderShippingMethod>()
                    .FindAsync(x => x.OrderId == order.Id, x => x.ShippingMethod!);
                
                var orderShippingMethod = orderShippingMethods.FirstOrDefault();
                if (orderShippingMethod?.ShippingMethod != null &&
                    Enum.TryParse<ShippingProviderType>(orderShippingMethod.ShippingMethod.Name, out var providerType))
                {
                    var shippingService = _shippingServiceFactory.GetShippingService(providerType);
                    await _callBackShareLogic.CreateShippingOrderAfterPaymentSuccessAsync(
                        order, shippingService, _unitOfWork, _logger, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPay IPN] Error creating shipping order after payment success for order {OrderId}", order.Id);
                // Don't throw - shipping order can be created manually later
            }

            // Save all changes together (order, payment, and shipping method updates)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            LogOrderAction(
                order,
                "[VNPay IPN] Order confirmed",
                new { order.OrderStatus, order.PaymentStatus, transaction.TransactionNo, transaction.Amount },
                cancellationToken);

            LogPaymentAction(
                order,
                payment,
                "[VNPay IPN] Payment completed",
                new { payment.PaymentStatus, payment.TransactionNo, payment.PaymentDate },
                cancellationToken);
            return Result<object>.Success(new { RspCode = "00", Message = "Success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay callback: {Message}", ex.Message);
            return Result<object>.Failure(ex.Message, ErrorCodeEnum.InternalError);
        }
    }
    private void LogOrderAction(
        Domain.Entities.Order order,
        string detail,
        object? newValue,
        CancellationToken cancellationToken)
    {
        var actorId = order.UserId ?? Guid.Empty;

        UserActionHelper.LogUserActionAsync(
            _serviceProvider,
            actorId,
            UserActionEnum.Update,
            order.Id,
            nameof(Domain.Entities.Order),
            detail,
            ipAddress: null,
            oldValue: null,
            newValue: newValue,
            cancellationToken: cancellationToken);
            }

    private void LogPaymentAction(
        Domain.Entities.Order order,
        Domain.Entities.Payment payment,
        string detail,
        object? newValue,
        CancellationToken cancellationToken)
        {
        var actorId = order.UserId ?? Guid.Empty;

        UserActionHelper.LogUserActionAsync(
            _serviceProvider,
            actorId,
            UserActionEnum.Update,
            payment.Id,
            nameof(Domain.Entities.Payment),
            detail,
            ipAddress: null,
            oldValue: null,
            newValue: newValue,
            cancellationToken: cancellationToken);
    }
}