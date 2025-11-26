using System;
using System.Collections.Generic;
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
using NekoViBE.Application.Common.QueryBuilders;

namespace NekoViBE.Application.Features.Order.Queries.GetCustomerOrderList;

public class GetCustomerOrderListQueryHandler
    : IRequestHandler<GetCustomerOrderListQuery, PaginationResult<CustomerOrderListItem>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCustomerOrderListQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomerOrderListQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCustomerOrderListQueryHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<PaginationResult<CustomerOrderListItem>> Handle(
        GetCustomerOrderListQuery request,
        CancellationToken cancellationToken)
    {
        var (isValid, currentUserId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || currentUserId == null)
        {
            return PaginationResult<CustomerOrderListItem>.Failure(
                "User is not authenticated",
                ErrorCodeEnum.Unauthorized);
        }

        request.Filter.UserId = currentUserId.Value;

        try
        {
            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var isAscending = request.Filter.IsAscending ?? false;

            // Use IQueryable directly to support ThenInclude for nested navigation properties
            // EF Core doesn't support .Select() in Include expressions, so we use ThenInclude instead
            var query = _unitOfWork.Repository<Domain.Entities.Order>().GetQueryable();
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // Include OrderItems with nested Product and ProductImages using ThenInclude
            query = query
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product!)
                        .ThenInclude(p => p.ProductImages!);

            // Apply ordering
            if (orderBy != null)
            {
                query = isAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var orders = await query
                .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
                .Take(request.Filter.PageSize)
                .ToListAsync(cancellationToken);

            var orderItems = _mapper.Map<List<CustomerOrderListItem>>(orders);
            // ProductImage is already converted to full URL by ProductImageUrlResolver

            return PaginationResult<CustomerOrderListItem>.Success(
                orderItems,
                request.Filter.Page,
                request.Filter.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer order list");
            return PaginationResult<CustomerOrderListItem>.Failure(
                "An error occurred while retrieving orders",
                ErrorCodeEnum.InternalError);
        }
    }
}

