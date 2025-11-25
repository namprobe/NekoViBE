using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers.PaymentHelper;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Payment.Commands;

public class ProcessVnPayCallbackCommandHandler : IRequestHandler<ProcessVnPayCallbackCommand, Result<object>>
{
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessVnPayCallbackCommandHandler> _logger;

    public ProcessVnPayCallbackCommandHandler(IPaymentGatewayFactory paymentGatewayFactory, IUnitOfWork unitOfWork, ILogger<ProcessVnPayCallbackCommandHandler> logger)
    {
        _paymentGatewayFactory = paymentGatewayFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
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
                    var failedOrder = await _unitOfWork.Repository<Domain.Entities.Order>()
                        .GetFirstOrDefaultAsync(x => x.Id == failedOrderId, o => o.Payment);
                    
                    if (failedOrder != null)
                    {
                        UpdateOrderAsFailed(failedOrder, paymentNote);
                        
                        if (failedOrder.Payment != null)
                        {
                            UpdatePaymentAsFailed(failedOrder.Payment, paymentNote, paymentResult.Message);
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
                .GetFirstOrDefaultAsync(x => x.Id == orderId, o => o.Payment);
            
            if (order == null)
            {
                throw new Exception($"Order not found for TnxRef: {transaction.TnxRef} (OrderId: {orderId})");
            }
            
            // Kiểm tra payment
            var payment = order.Payment;
            
            if (payment == null)
            {
                // Update order fail và save changes trước khi throw
                UpdateOrderAsFailed(order, $"{paymentNote} | Payment record not found");
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

    /// <summary>
    /// Update order thành failed
    /// </summary>
    private void UpdateOrderAsFailed(Domain.Entities.Order order, string note)
    {
        order.OrderStatus = OrderStatusEnum.Cancelled;
        order.PaymentStatus = PaymentStatusEnum.Failed;
        order.Notes = note;
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Domain.Entities.Order>().Update(order);
    }

    /// <summary>
    /// Update payment thành failed
    /// </summary>
    private void UpdatePaymentAsFailed(Domain.Entities.Payment payment, string note, string processorResponse)
    {
        payment.PaymentStatus = PaymentStatusEnum.Failed;
        payment.Notes = note;
        payment.ProcessorResponse = processorResponse;
        payment.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Domain.Entities.Payment>().Update(payment);
    }
}