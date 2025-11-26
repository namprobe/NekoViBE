using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
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
    private readonly IOrderRollbackService _orderRollbackService;

    public ProcessVnPayCallbackCommandHandler(
        IPaymentGatewayFactory paymentGatewayFactory, 
        IUnitOfWork unitOfWork, 
        ILogger<ProcessVnPayCallbackCommandHandler> logger,
        IOrderRollbackService orderRollbackService)
    {
        _paymentGatewayFactory = paymentGatewayFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _orderRollbackService = orderRollbackService;
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
                            o => o.OrderItems, 
                            o => o.UserCoupons);
                    
                    if (failedOrder != null)
                    {
                        // Revert stock và coupon trước khi update order status
                        await _orderRollbackService.RevertOrderChangesAsync(
                            failedOrder, _unitOfWork, _logger, cancellationToken);
                        
                        _orderRollbackService.UpdateOrderAsFailed(failedOrder, paymentNote, _unitOfWork);
                        
                        if (failedOrder.Payment != null)
                        {
                            _orderRollbackService.UpdatePaymentAsFailed(
                                failedOrder.Payment, paymentNote, paymentResult.Message, _unitOfWork);
                        }
                        
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
                await _orderRollbackService.RevertOrderChangesAsync(
                    order, _unitOfWork, _logger, cancellationToken);
                
                // Update order fail và save changes trước khi throw
                _orderRollbackService.UpdateOrderAsFailed(
                    order, $"{paymentNote} | Payment record not found", _unitOfWork);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                throw new Exception($"Payment not found for Order: {order.Id} (TnxRef: {transaction.TnxRef})");
            }
            
            // Cập nhật trạng thái order và payment thành công
            order.OrderStatus = OrderStatusEnum.Confirmed;
            order.PaymentStatus = PaymentStatusEnum.Completed;
            order.Notes = paymentNote;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Domain.Entities.Order>().Update(order);
            
            payment.TransactionNo = transaction.TransactionNo;
            payment.PaymentStatus = PaymentStatusEnum.Completed;
            payment.PaymentDate = transaction.PaymentDate;
            payment.Notes = paymentNote;
            payment.ProcessorResponse = paymentResult.Message;
            payment.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Domain.Entities.Payment>().Update(payment);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<object>.Success(new { RspCode = "00", Message = "Success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay callback: {Message}", ex.Message);
            return Result<object>.Failure(ex.Message, ErrorCodeEnum.InternalError);
        }
    }
}