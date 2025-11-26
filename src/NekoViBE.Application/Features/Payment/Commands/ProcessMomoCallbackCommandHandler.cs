using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers.PaymentHelper;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.Models.Momo;
using NekoViBE.Application.Features.Payment.Services;
using NekoViBE.Domain.Enums;
using PaymentService.Application.Commons.Models.Momo;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace NekoViBE.Application.Features.Payment.Commands;

public class ProcessMomoCallbackCommandHandler : IRequestHandler<ProcessMomoCallbackCommand, Result<MoMoIpnResponse>>
{
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessMomoCallbackCommandHandler> _logger;
    private readonly IOrderRollbackService _orderRollbackService;
    private readonly HashSet<string> _ipWhitelist;

    public ProcessMomoCallbackCommandHandler(
        IPaymentGatewayFactory paymentGatewayFactory,
        IUnitOfWork unitOfWork,
        ILogger<ProcessMomoCallbackCommandHandler> logger,
        IOptions<MoMoSettings> moMoSettings,
        IOrderRollbackService orderRollbackService)
    {
        _paymentGatewayFactory = paymentGatewayFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _orderRollbackService = orderRollbackService;
        var whitelistConfig = moMoSettings.Value.MomoIpnWhitelist;
        _ipWhitelist = whitelistConfig?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ip => ip.Trim())
            .Where(ip => !string.IsNullOrWhiteSpace(ip))
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<Result<MoMoIpnResponse>> Handle(ProcessMomoCallbackCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[MoMo IPN] Processing started - OrderId: {OrderId}, TransId: {TransId}, ResultCode: {ResultCode}, CallerIP: {CallerIP}",
            request.Request?.OrderId, request.Request?.TransId, request.Request?.ResultCode, request.CallerIpAddress);
        
        try
        {
            if (!ValidateMomoIpWhitelist(request.CallerIpAddress))
            {
                _logger.LogWarning(
                    "[MoMo IPN] REJECTED - Unauthorized IP address: {CallerIP}, OrderId: {OrderId}",
                    request.CallerIpAddress, request.Request?.OrderId);
                var unauthorizedResponse = BuildIpnResponse(request.Request, 99, "Unauthorized IP address");
                return Result<MoMoIpnResponse>.Success(unauthorizedResponse, "Unauthorized IP address");
            }

            _logger.LogDebug("[MoMo IPN] IP whitelist validation passed for IP: {CallerIP}", request.CallerIpAddress);

            var paymentGateway = _paymentGatewayFactory.GetPaymentGatewayService(PaymentGatewayType.Momo);
            var paymentResult = paymentGateway.VerifyIpnRequest(request.Request);
            
            _logger.LogInformation(
                "[MoMo IPN] Signature verification - IsSuccess: {IsSuccess}, Message: {Message}",
                paymentResult.IsSuccess, paymentResult.Message);
            
            // Lấy thông tin transaction
            var transaction = paymentResult.Data as MomoTransactionResponse;
            
            // Generate note message từ payment result
            var paymentNote = PaymentNoteHelper.GeneratePaymentNote(paymentResult, transaction);
            
            // Xác định resultCode và message cho response
            int resultCode;
            string message;
            
            // Kiểm tra payment result: nếu fail, update order và payment (nếu có) thành failed
            if (!paymentResult.IsSuccess)
            {
                resultCode = transaction?.ResultCode ?? 99; // Ưu tiên trả về mã lỗi từ MoMo
                message = paymentResult.Message;
                
                _logger.LogWarning(
                    "[MoMo IPN] Payment failed - OrderId: {OrderId}, ResultCode: {ResultCode}, Message: {Message}",
                    transaction?.OrderId, resultCode, message);
                
                if (transaction != null && !string.IsNullOrEmpty(transaction.OrderId))
                {
                    if (Guid.TryParse(transaction.OrderId, out var failedOrderId))
                    {
                        _logger.LogInformation("[MoMo IPN] Looking up failed order: {OrderId}", failedOrderId);
                        
                        // QUAN TRỌNG: chỉ update order status thành failed nếu order status là Processing
                        var failedOrder = await _unitOfWork.Repository<Domain.Entities.Order>()
                            .GetFirstOrDefaultAsync(x => x.Id == failedOrderId && x.OrderStatus == OrderStatusEnum.Processing, o => o.Payment);
                        
                        if (failedOrder != null)
                        {
                            _logger.LogInformation(
                                "[MoMo IPN] Updating order as failed - OrderId: {OrderId}, HasPayment: {HasPayment}",
                                failedOrderId, failedOrder.Payment != null);
                            
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
                            _logger.LogInformation("[MoMo IPN] Order and payment updated as failed - OrderId: {OrderId}", failedOrderId);
                        }
                        else
                        {
                            _logger.LogWarning("[MoMo IPN] Order not found for failed payment - OrderId: {OrderId}", failedOrderId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("[MoMo IPN] Invalid OrderId format for failed payment: {OrderId}", transaction.OrderId);
                    }
                }
                
                // Trả về response với resultCode = 99 (lỗi)
                var errorResponse = BuildIpnResponse(request.Request, resultCode, message);
                _logger.LogInformation("[MoMo IPN] Returning error response - ResultCode: {ResultCode}", resultCode);
                return Result<MoMoIpnResponse>.Success(errorResponse);
            }
            
            // Payment result success: kiểm tra transaction null
            if (transaction == null)
            {
                _logger.LogError("[MoMo IPN] Transaction response is null but payment result is success - OrderId: {OrderId}", request.Request?.OrderId);
                resultCode = 99;
                message = "Transaction response is null but payment result is success";
                var nullResponse = BuildIpnResponse(request.Request, resultCode, message);
                return Result<MoMoIpnResponse>.Success(nullResponse);
            }
            
            _logger.LogInformation(
                "[MoMo IPN] Payment verification successful - OrderId: {OrderId}, TransId: {TransId}, Amount: {Amount}",
                transaction.OrderId, transaction.TransId, transaction.Amount);
            
            // Kiểm tra order
            if (!Guid.TryParse(transaction.OrderId, out var orderId))
            {
                _logger.LogError("[MoMo IPN] Invalid OrderId format: {OrderId}", transaction.OrderId);
                resultCode = 42; // OrderId không hợp lệ
                message = $"Invalid OrderId format: {transaction.OrderId}";
                var invalidResponse = BuildIpnResponse(request.Request, resultCode, message);
                return Result<MoMoIpnResponse>.Success(invalidResponse);
            }
            
            _logger.LogInformation("[MoMo IPN] Looking up order: {OrderId}", orderId);
            
            var order = await _unitOfWork.Repository<Domain.Entities.Order>()
                .GetFirstOrDefaultAsync(x => x.Id == orderId && x.OrderStatus == OrderStatusEnum.Processing, o => o.Payment);
            // QUAN TRỌNG: chỉ update order status thành failed hoặc confirmed nếu order status là Processing
            
            if (order == null)
            {
                _logger.LogError("[MoMo IPN] Order not found - OrderId: {OrderId}", orderId);
                resultCode = 42; // OrderId không được tìm thấy
                message = $"Order not found for OrderId: {transaction.OrderId}";
                var notFoundResponse = BuildIpnResponse(request.Request, resultCode, message);
                return Result<MoMoIpnResponse>.Success(notFoundResponse);
            }
            
            _logger.LogInformation(
                "[MoMo IPN] Order found - OrderId: {OrderId}, CurrentStatus: {OrderStatus}, PaymentStatus: {PaymentStatus}, HasPayment: {HasPayment}",
                order.Id, order.OrderStatus, order.PaymentStatus, order.Payment != null);
            
            // Kiểm tra payment
            var payment = order.Payment;
            
            if (payment == null)
            {
                _logger.LogError("[MoMo IPN] Payment record not found for order - OrderId: {OrderId}", order.Id);
                
                // Revert stock và coupon trước khi update order status
                await _orderRollbackService.RevertOrderChangesAsync(
                    order, _unitOfWork, _logger, cancellationToken);
                
                // Update order fail và save changes trước khi trả response
                _orderRollbackService.UpdateOrderAsFailed(
                    order, $"{paymentNote} | Payment record not found", _unitOfWork);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                resultCode = 99;
                message = $"Payment not found for Order: {order.Id}";
                var noPaymentResponse = BuildIpnResponse(request.Request, resultCode, message);
                return Result<MoMoIpnResponse>.Success(noPaymentResponse);
            }
            
            // Cập nhật trạng thái order và payment thành công
            _logger.LogInformation(
                "[MoMo IPN] Updating order and payment to success - OrderId: {OrderId}, TransId: {TransId}",
                order.Id, transaction.TransId);
            
            order.OrderStatus = OrderStatusEnum.Confirmed;
            order.PaymentStatus = PaymentStatusEnum.Completed;
            order.Notes = paymentNote;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Domain.Entities.Order>().Update(order);
            
            payment.TransactionNo = transaction.TransId;
            payment.PaymentStatus = PaymentStatusEnum.Completed;
            payment.PaymentDate = transaction.ResponseTime;
            payment.Notes = paymentNote;
            payment.ProcessorResponse = paymentResult.Message;
            payment.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Domain.Entities.Payment>().Update(payment);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "[MoMo IPN] Order and payment updated successfully - OrderId: {OrderId}, TransId: {TransId}, Amount: {Amount}",
                order.Id, transaction.TransId, transaction.Amount);
            
            // Trả về response thành công
            resultCode = 0; // Giao dịch thành công
            message = "Success";
            var successResponse = BuildIpnResponse(request.Request, resultCode, message);
            _logger.LogInformation("[MoMo IPN] Returning success response - OrderId: {OrderId}", order.Id);
            return Result<MoMoIpnResponse>.Success(successResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[MoMo IPN] EXCEPTION - OrderId: {OrderId}, TransId: {TransId}, Error: {Error}",
                request.Request?.OrderId, request.Request?.TransId, ex.Message);
            
            // Trả về response lỗi
            var errorResponse = BuildIpnResponse(request.Request, 99, $"Lỗi xử lý: {ex.Message}");
            _logger.LogInformation("[MoMo IPN] Returning error response due to exception - OrderId: {OrderId}", request.Request?.OrderId);
            return Result<MoMoIpnResponse>.Success(errorResponse);
        }
    }

    /// <summary>
    /// Build MoMo IPN response với signature
    /// </summary>
    private MoMoIpnResponse BuildIpnResponse(MoMoIpnRequest request, int resultCode, string message)
    {
        var responseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        var response = new MoMoIpnResponse
        {
            PartnerCode = request.PartnerCode,
            RequestId = request.RequestId,
            OrderId = request.OrderId,
            ResultCode = resultCode,
            Message = message,
            ResponseTime = responseTime,
            ExtraData = request.ExtraData ?? string.Empty
        };
        
        // Build signature cho response
        var paymentGateway = _paymentGatewayFactory.GetPaymentGatewayService(PaymentGatewayType.Momo);
        response.Signature = paymentGateway.BuildIpnResponseSignature(response);
        
        return response;
    }

    private bool ValidateMomoIpWhitelist(string? ipAddress)
    {
        if (_ipWhitelist.Count == 0)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return false;
        }

        if (_ipWhitelist.Contains(ipAddress))
        {
            return true;
        }

        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            return false;
        }

        foreach (var entry in _ipWhitelist)
        {
            if (entry.Contains('/'))
            {
                if (IsIpInCidrRange(ip, entry))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsIpInCidrRange(IPAddress ipAddress, string cidr)
    {
        try
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2)
            {
                return false;
            }

            var baseAddress = IPAddress.Parse(parts[0]);
            var prefixLength = int.Parse(parts[1]);

            var ipBytes = ipAddress.GetAddressBytes();
            var baseBytes = baseAddress.GetAddressBytes();

            if (ipBytes.Length != baseBytes.Length)
            {
                return false;
            }

            var fullBytes = prefixLength / 8;
            var remainingBits = prefixLength % 8;

            for (var i = 0; i < fullBytes; i++)
            {
                if (ipBytes[i] != baseBytes[i])
                {
                    return false;
                }
            }

            if (remainingBits > 0)
            {
                var mask = (byte)(0xFF << (8 - remainingBits));
                if ((ipBytes[fullBytes] & mask) != (baseBytes[fullBytes] & mask))
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}

