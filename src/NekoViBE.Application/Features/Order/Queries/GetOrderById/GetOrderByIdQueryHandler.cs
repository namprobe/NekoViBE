using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Order.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrderByIdQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetOrderByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetOrderByIdQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var (isValid, currentUserId, userRoles) = await _currentUserService.ValidateUserWithRolesAsync();
            if (!isValid || currentUserId == null)
            {
                return Result<OrderDto>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
            }

            try
            {
                var order = await _unitOfWork.Repository<Domain.Entities.Order>()
                    .GetQueryable()
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.ProductImages)
                    .Include(o => o.OrderShippingMethods)
                        .ThenInclude(osm => osm.ShippingMethod)
                    .Include(o => o.OrderShippingMethods)
                        .ThenInclude(osm => osm.UserAddress)
                    .Include(o => o.Payment)
                        .ThenInclude(p => p!.PaymentMethod)
                    .Include(o => o.UserCoupons)
                        .ThenInclude(uc => uc.Coupon)
                    .FirstOrDefaultAsync(o => o.Id == request.Id && o.Status == Domain.Enums.EntityStatusEnum.Active, cancellationToken);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", request.Id);
                    return Result<OrderDto>.Failure("Order not found", ErrorCodeEnum.NotFound);
                }

                var orderDto = _mapper.Map<OrderDto>(order);

                //_logger.LogInformation("Order {OrderId} retrieved successfully by user {UserId}", request.Id, currentUserId);
                return Result<OrderDto>.Success(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", request.Id);
                return Result<OrderDto>.Failure("An error occurred while retrieving the order", ErrorCodeEnum.InternalError);
            }
        }
    }
}

