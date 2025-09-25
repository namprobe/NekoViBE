using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Order.Commands.CreateOrder;


namespace NekoViBE.Application.Features.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result>
{
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrderService _orderService;

    public CreateOrderCommandHandler(
        ILogger<CreateOrderCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IOrderService orderService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _orderService = orderService;
    }

    public async Task<Result> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = command.Request;

            // Validate user (for authenticated users)
            Guid? userId = null;
            if (!request.IsOneClick && string.IsNullOrEmpty(request.GuestEmail))
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }
                userId = currentUserId;
            }

            // Use OrderService to handle complex order creation logic
            var orderResult = await _orderService.CreateOrderAsync(request, userId, cancellationToken);

            if (!orderResult.IsSuccess)
            {
                return Result.Failure(orderResult.Message, orderResult.ErrorCode);
            }

            var orderDto = _mapper.Map<OrderDto>(orderResult.Data);
            return Result.Success("Order created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return Result.Failure("Error creating order", ErrorCodeEnum.InternalError);
        }
    }
}