using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Order.Queries.GetOrderList
{
    public class GetOrderListQueryHandler : IRequestHandler<GetOrderListQuery, PaginationResult<OrderListItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrderListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetOrderListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetOrderListQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<OrderListItem>> Handle(GetOrderListQuery request, CancellationToken cancellationToken)
        {
            var (isValid, currentUserId, userRoles) = await _currentUserService.ValidateUserWithRolesAsync();
            if (!isValid || currentUserId == null)
            {
                return PaginationResult<OrderListItem>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
            }

            try
            {
                // Use extension methods for filtering and ordering
                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                // Define includes for related entities
                var includes = new Expression<Func<Domain.Entities.Order, object>>[]
                {
                    o => o.User!,
                    o => o.OrderItems!,
                    o => o.UserCoupons!
                };

                var (orders, totalCount) = await _unitOfWork.Repository<Domain.Entities.Order>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending,
                    includes: includes);

                var orderItems = _mapper.Map<List<OrderListItem>>(orders);

                _logger.LogInformation("Order list retrieved successfully by {CurrentUser}, TotalItems: {TotalCount}",
                    currentUserId, totalCount);

                return PaginationResult<OrderListItem>.Success(
                    orderItems,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order list");
                return PaginationResult<OrderListItem>.Failure("An error occurred while retrieving orders", ErrorCodeEnum.InternalError);
            }
        }
    }
}
