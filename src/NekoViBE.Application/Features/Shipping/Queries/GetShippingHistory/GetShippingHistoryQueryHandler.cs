using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Shipping;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Shipping.Queries.GetShippingHistory;

public class GetShippingHistoryQueryHandler
    : IRequestHandler<GetShippingHistoryQuery, Result<List<ShippingHistoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetShippingHistoryQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetShippingHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetShippingHistoryQueryHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ShippingHistoryDto>>> Handle(
        GetShippingHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var (isValid, currentUserId, userRoles) = await _currentUserService.ValidateUserWithRolesAsync();
        if (!isValid || currentUserId == null)
        {
            return Result<List<ShippingHistoryDto>>.Failure(
                "User is not authenticated",
                ErrorCodeEnum.Unauthorized);
        }

        try
        {
            // Check if user is Admin or Staff (can view any order)
            var isAdminOrStaff = userRoles?.Any(r => r == Domain.Enums.RoleEnum.Admin || r == Domain.Enums.RoleEnum.Staff) ?? false;
            
            // Verify order exists and belongs to current user (if customer)
            Domain.Entities.Order? order;
            if (isAdminOrStaff)
            {
                // Admin/Staff can view any order
                order = await _unitOfWork.Repository<Domain.Entities.Order>()
                    .GetFirstOrDefaultAsync(x => x.Id == request.OrderId);
            }
            else
            {
                // Customer can only view their own orders
                order = await _unitOfWork.Repository<Domain.Entities.Order>()
                    .GetFirstOrDefaultAsync(x => x.Id == request.OrderId && x.UserId == currentUserId.Value);
            }

            if (order == null)
            {
                return Result<List<ShippingHistoryDto>>.Failure(
                    "Order not found",
                    ErrorCodeEnum.NotFound);
            }

            // Get shipping history for this order, ordered by event time (newest first)
            var histories = await _unitOfWork.Repository<Domain.Entities.ShippingHistory>()
                .GetQueryable()
                .Where(x => x.OrderId == request.OrderId)
                .OrderByDescending(x => x.EventTime)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<ShippingHistoryDto>>(histories);
            return Result<List<ShippingHistoryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipping history for OrderId {OrderId}", request.OrderId);
            return Result<List<ShippingHistoryDto>>.Failure(
                "An error occurred while retrieving shipping history",
                ErrorCodeEnum.InternalError);
        }
    }
}

