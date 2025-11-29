using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.Models.GHN;
using NekoViBE.Application.Features.Payment.Services;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Shipping.Commands;

public class ProcessGHNCallbackCommandHandler : IRequestHandler<ProcessGHNCallbackCommand, Result<object>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessGHNCallbackCommandHandler> _logger;
    private readonly IShippingServiceFactory _shippingServiceFactory;
    private readonly IServiceProvider _serviceProvider;

    public ProcessGHNCallbackCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ProcessGHNCallbackCommandHandler> logger,
        IShippingServiceFactory shippingServiceFactory,
        IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _shippingServiceFactory = shippingServiceFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<object>> Handle(ProcessGHNCallbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Request == null)
            {
                _logger.LogWarning("[GHN Callback] Received null request");
                return Result<object>.Failure("Request body is required", ErrorCodeEnum.InternalError);
            }

            var ghnRequest = request.Request;
            _logger.LogInformation(
                "[GHN Callback] Received callback - OrderCode: {OrderCode}, ClientOrderCode: {ClientOrderCode}, Status (string): {Status}, Type: {Type}, Time: {Time}, CallerIP: {CallerIP}",
                ghnRequest.OrderCode, ghnRequest.ClientOrderCode, ghnRequest.Status, ghnRequest.Type, ghnRequest.Time, request.CallerIpAddress);

            // Get shipping service to handle callback
            // GHN sends Status as string (e.g., "ready_to_pick", "delivered")
            // MapGHNStatus will convert it to internal int status
            var shippingService = _shippingServiceFactory.GetShippingService(ShippingProviderType.GHN);
            var callbackResult = shippingService.HandleCallback(ghnRequest);

            if (!callbackResult.IsSuccess || callbackResult.Data == null)
            {
                _logger.LogWarning("[GHN Callback] Failed to process callback: {Message}", callbackResult.Message);
                return Result<object>.Success(new { code = 200, message = "Callback received but processing failed" });
            }

            var callbackData = callbackResult.Data;
            
            _logger.LogInformation(
                "[GHN Callback] Status mapped - GHN Status (string): {GhnStatus}, Internal Status (int): {InternalStatus}, StatusName: {StatusName}",
                ghnRequest.Status, callbackData.Status, callbackData.StatusName);

            // Find OrderShippingMethod by tracking number (GHN OrderCode)
            var orderShippingMethod = await _unitOfWork.Repository<Domain.Entities.OrderShippingMethod>()
                .GetFirstOrDefaultAsync(
                    x => x.TrackingNumber == callbackData.OrderCode,
                    x => x.Order!,
                    x => x.ShippingMethod!);

            if (orderShippingMethod == null)
            {
                _logger.LogWarning(
                    "[GHN Callback] OrderShippingMethod not found for tracking number {TrackingNumber}",
                    callbackData.OrderCode);
                return Result<object>.Success(new { code = 200, message = "OrderShippingMethod not found" });
            }

            var order = orderShippingMethod.Order;
            if (order == null)
            {
                _logger.LogWarning(
                    "[GHN Callback] Order not found for OrderShippingMethod {OrderShippingMethodId}",
                    orderShippingMethod.Id);
                return Result<object>.Success(new { code = 200, message = "Order not found" });
            }

            _logger.LogInformation(
                "[GHN Callback] Found order - OrderId: {OrderId}, CurrentOrderStatus: {OrderStatus}, CurrentTrackingNumber: {TrackingNumber}",
                order.Id, order.OrderStatus, orderShippingMethod.TrackingNumber);

            // Update OrderShippingMethod with new status and dates
            var previousOrderStatus = order.OrderStatus; // Store for logging
            orderShippingMethod.UpdatedAt = callbackData.UpdatedAt ?? DateTime.UtcNow;

            // Update shipped/delivered dates based on status
            if (callbackData.Status == 5) // "picked" - Đã lấy hàng
            {
                if (!orderShippingMethod.ShippedDate.HasValue)
                {
                    orderShippingMethod.ShippedDate = callbackData.UpdatedAt ?? DateTime.UtcNow;
                }
            }
            else if (callbackData.Status == 11) // "delivered" - Đã giao hàng
            {
                if (!orderShippingMethod.DeliveredDate.HasValue)
                {
                    orderShippingMethod.DeliveredDate = callbackData.UpdatedAt ?? DateTime.UtcNow;
                }
            }

            _unitOfWork.Repository<Domain.Entities.OrderShippingMethod>().Update(orderShippingMethod);

            // Update Order status based on shipping status
            // Reference: GHN status mapping from MapGHNStatus method
            var shouldUpdateOrderStatus = false;
            var newOrderStatus = order.OrderStatus;

            switch (callbackData.Status)
            {
                // Status 1-4: Chờ lấy hàng, Đang lấy hàng, Đã hủy, Đang thu tiền người gửi
                case 1: // "ready_to_pick" - Chờ lấy hàng
                case 2: // "picking" - Đang lấy hàng
                case 4: // "money_collect_picking" - Đang thu tiền người gửi
                    // Không cần update order status, vẫn giữ Confirmed
                    break;

                case 3: // "cancel" - Đã hủy
                    if (order.OrderStatus != OrderStatusEnum.Cancelled && order.OrderStatus != OrderStatusEnum.Failed)
                    {
                        newOrderStatus = OrderStatusEnum.Cancelled;
                        shouldUpdateOrderStatus = true;
                    }
                    break;

                // Status 5-10: Đã lấy hàng, Đang lưu kho, Đang vận chuyển, Đang phân loại, Đang giao hàng, Đang thu tiền người nhận
                case 5: // "picked" - Đã lấy hàng
                case 6: // "storing" - Đang lưu kho
                case 7: // "transporting" - Đang vận chuyển
                case 8: // "sorting" - Đang phân loại
                case 9: // "delivering" - Đang giao hàng
                case 10: // "money_collect_delivering" - Đang thu tiền người nhận
                    // Update to Shipping status if order is still Confirmed
                    if (order.OrderStatus == OrderStatusEnum.Confirmed)
                    {
                        newOrderStatus = OrderStatusEnum.Shipping;
                        shouldUpdateOrderStatus = true;
                    }
                    // Nếu đã là Shipping rồi thì không cần update lại
                    break;

                case 11: // "delivered" - Đã giao hàng
                    if (order.OrderStatus == OrderStatusEnum.Shipping || order.OrderStatus == OrderStatusEnum.Confirmed)
                    {
                        newOrderStatus = OrderStatusEnum.Delivered;
                        shouldUpdateOrderStatus = true;
                    }
                    break;

                case 12: // "delivery_fail" - Giao hàng thất bại
                    if (order.OrderStatus != OrderStatusEnum.Cancelled && order.OrderStatus != OrderStatusEnum.Failed)
                    {
                        newOrderStatus = OrderStatusEnum.Failed;
                        shouldUpdateOrderStatus = true;
                    }
                    break;

                // Status 13-18: Chờ trả hàng, Đang trả hàng, Đang vận chuyển trả hàng, Đang phân loại trả hàng, Đang trả hàng, Trả hàng thất bại
                case 13: // "waiting_to_return" - Chờ trả hàng
                case 14: // "return" - Đang trả hàng
                case 15: // "return_transporting" - Đang vận chuyển trả hàng
                case 16: // "return_sorting" - Đang phân loại trả hàng
                case 17: // "returning" - Đang trả hàng
                    // Có thể giữ nguyên status hiện tại hoặc chuyển sang một status trung gian
                    // Tạm thời không update, chỉ log
                    break;

                case 18: // "return_fail" - Trả hàng thất bại
                    // Có thể xử lý như failed hoặc giữ nguyên
                    // Tạm thời không update, chỉ log
                    break;

                case 19: // "returned" - Đã trả hàng
                    if (order.OrderStatus != OrderStatusEnum.Returned)
                    {
                        newOrderStatus = OrderStatusEnum.Returned;
                        shouldUpdateOrderStatus = true;
                    }
                    break;

                // Status 20-22: Ngoại lệ, Hàng hỏng, Hàng mất
                case 20: // "exception" - Ngoại lệ
                case 21: // "damage" - Hàng hỏng
                case 22: // "lost" - Hàng mất
                    // Các trường hợp đặc biệt, có thể cần xử lý riêng
                    // Tạm thời update thành Failed
                    if (order.OrderStatus != OrderStatusEnum.Cancelled && order.OrderStatus != OrderStatusEnum.Failed)
                    {
                        newOrderStatus = OrderStatusEnum.Failed;
                        shouldUpdateOrderStatus = true;
                    }
                    break;

                default:
                    _logger.LogWarning(
                        "[GHN Callback] Unknown status code: {Status} for OrderCode: {OrderCode}",
                        callbackData.Status, callbackData.OrderCode);
                    break;
            }

            if (shouldUpdateOrderStatus)
            {
                order.OrderStatus = newOrderStatus;
                order.UpdatedAt = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(order.Notes))
                {
                    order.Notes = $"GHN Status: {callbackData.StatusName}";
                }
                else if (!order.Notes.Contains(callbackData.StatusName))
                {
                    order.Notes += $" | GHN Status: {callbackData.StatusName}";
                }
                _unitOfWork.Repository<Domain.Entities.Order>().Update(order);

                _logger.LogInformation(
                    "[GHN Callback] Updated order status - OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                    order.Id, previousOrderStatus, newOrderStatus);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            LogOrderAction(
                order,
                $"[GHN Callback] Shipping status updated: {callbackData.StatusName} (Type: {ghnRequest.Type})",
                request.CallerIpAddress,
                new
                {
                    OrderStatus = order.OrderStatus,
                    ShippingStatus = callbackData.StatusName,
                    CallbackType = ghnRequest.Type,
                    TrackingNumber = callbackData.OrderCode,
                    TotalFee = ghnRequest.TotalFee,
                    CODAmount = ghnRequest.CODAmount,
                    UpdatedAt = callbackData.UpdatedAt
                },
                cancellationToken);

            _logger.LogInformation(
                "[GHN Callback] Successfully processed callback - OrderCode: {OrderCode}, OrderId: {OrderId}, Status: {Status}",
                callbackData.OrderCode, order.Id, callbackData.StatusName);

            // GHN expects HTTP 200 response
            return Result<object>.Success(new { code = 200, message = "Success", data = callbackData });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GHN Callback] EXCEPTION - OrderCode: {OrderCode}, Error: {Error}",
                request.Request?.OrderCode, ex.Message);

            // GHN expects HTTP 200 even on error
            return Result<object>.Success(new { code = 200, message = $"Error processing callback: {ex.Message}" });
        }
    }

    private void LogOrderAction(
        Domain.Entities.Order order,
        string detail,
        string? ipAddress,
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
            ipAddress,
            oldValue: null,
            newValue: newValue,
            cancellationToken: cancellationToken);
    }
}

