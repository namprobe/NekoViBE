using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.OrderItem;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.OrderItem.Query.GetOrderItemsByOrderId
{
    public class GetOrderItemsByOrderIdQueryHandler : IRequestHandler<GetOrderItemsByOrderIdQuery, Result<List<OrderItemDetailDTO>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrderItemsByOrderIdQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetOrderItemsByOrderIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetOrderItemsByOrderIdQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result<List<OrderItemDetailDTO>>> Handle(GetOrderItemsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate current user
                var (isValid, currentUserId, userRoles) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid || currentUserId == null)
                {
                    return Result<List<OrderItemDetailDTO>>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
                }

                // Check if order exists and user has permission
                var order = await _unitOfWork.Repository<Domain.Entities.Order>()
                    .GetFirstOrDefaultAsync(o => o.Id == request.OrderId,
                        includes: new Expression<Func<Domain.Entities.Order, object>>[] { o => o.User! });

                if (order == null)
                {
                    return Result<List<OrderItemDetailDTO>>.Failure("Order not found", ErrorCodeEnum.NotFound);
                }

                // Check permission: user can only see their own orders unless they are admin/staff
                var isAdminOrStaff = userRoles?.Any(r => r == RoleEnum.Admin || r == RoleEnum.Staff) ?? false;
                var isOrderOwner = order.UserId == currentUserId;

                if (!isAdminOrStaff && !isOrderOwner)
                {
                    return Result<List<OrderItemDetailDTO>>.Failure("Access denied", ErrorCodeEnum.Forbidden);
                }

                // Get order items with product information
                var orderItems = await _unitOfWork.Repository<Domain.Entities.OrderItem>()
                    .FindAsync(oi => oi.OrderId == request.OrderId,
                        includes: new Expression<Func<Domain.Entities.OrderItem, object>>[]
                        {
                            oi => oi.Product!,
                            oi => oi.Product!.ProductImages!
                        });

                var orderItemDTOs = _mapper.Map<List<OrderItemDetailDTO>>(orderItems);

                _logger.LogInformation("Retrieved {Count} order items for order {OrderId} by user {UserId}",
                    orderItemDTOs.Count, request.OrderId, currentUserId);

                return Result<List<OrderItemDetailDTO>>.Success(orderItemDTOs, "Order items retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for order {OrderId}", request.OrderId);
                return Result<List<OrderItemDetailDTO>>.Failure("An error occurred while retrieving order items", ErrorCodeEnum.InternalError);
            }
        }
    }
}
