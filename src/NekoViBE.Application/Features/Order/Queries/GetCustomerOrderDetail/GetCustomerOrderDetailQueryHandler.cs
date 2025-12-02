using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Order.Queries.GetCustomerOrderDetail;

public class GetCustomerOrderDetailQueryHandler
    : IRequestHandler<GetCustomerOrderDetailQuery, Result<CustomerOrderDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomerOrderDetailQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomerOrderDetailQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCustomerOrderDetailQueryHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CustomerOrderDetailDto>> Handle(
        GetCustomerOrderDetailQuery request,
        CancellationToken cancellationToken)
    {
        var (isValid, currentUserId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || currentUserId == null)
        {
            return Result<CustomerOrderDetailDto>.Failure(
                "User is not authenticated",
                ErrorCodeEnum.Unauthorized);
        }

        try
        {
            Expression<Func<Domain.Entities.Order, bool>> predicate = order =>
                order.Id == request.OrderId &&
                order.UserId == currentUserId.Value;

            // Use IQueryable directly to support ThenInclude for nested navigation properties
            // EF Core doesn't support .Select() in Include expressions, so we use ThenInclude instead
            var query = _unitOfWork.Repository<Domain.Entities.Order>().GetQueryable()
                .Where(predicate)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.ProductImages!)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.Category!)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.AnimeSeries!)
                .Include(o => o.Payment!)
                    .ThenInclude(p => p.PaymentMethod!)
                .Include(o => o.OrderShippingMethods!)
                    .ThenInclude(os => os.ShippingMethod!)
                .Include(o => o.UserCoupons!)
                    .ThenInclude(uc => uc.Coupon!);

            var order = await query.FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return Result<CustomerOrderDetailDto>.Failure(
                    "Order not found",
                    ErrorCodeEnum.NotFound);
            }

            var dto = _mapper.Map<CustomerOrderDetailDto>(order);
            dto.Shipping = MapShippingInfo(order);
            // ProductImage is already converted to full URL by ProductImageUrlResolver
            return Result<CustomerOrderDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order detail for {OrderId}", request.OrderId);
            return Result<CustomerOrderDetailDto>.Failure(
                "An error occurred while retrieving order detail",
                ErrorCodeEnum.InternalError);
        }
    }

    private static CustomerOrderShippingDto? MapShippingInfo(Domain.Entities.Order order)
    {
        var shipping = order.OrderShippingMethods?
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (shipping == null)
        {
            return null;
        }

        return new CustomerOrderShippingDto
        {
            ShippingMethodName = shipping.ShippingMethod?.Name,
            TrackingNumber = shipping.TrackingNumber,
            ShippedDate = shipping.ShippedDate,
            EstimatedDeliveryDate = shipping.EstimatedDeliveryDate,
            DeliveredDate = shipping.DeliveredDate,
            ShippingStatus = order.OrderStatus
        };
    }
}

