using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Features.Cart.Commands.ClearCart;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ClearCartCommandHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ClearCartCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<ClearCartCommandHandler> logger, IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }
            var cart = await _unitOfWork.Repository<ShoppingCart>()
                .GetFirstOrDefaultAsync(c => c.UserId == userId, c => c.CartItems);
            if (cart == null)
                return Result.Failure("Cart not found", ErrorCodeEnum.NotFound);
            
            // Tối ưu: Dùng DeleteRange thay vì Clear() vì repo dùng AsNoTracking
            if (cart.CartItems.Any())
            {
                _unitOfWork.Repository<CartItem>().DeleteRange(cart.CartItems);
                cart.UpdateEntity(userId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                // Log user action (fire and forget)
                UserActionHelper.LogUserActionAsync(
                    _serviceProvider, 
                    userId.Value, 
                    UserActionEnum.Delete, 
                    cart.Id, 
                    "Cart", 
                    "Cart cleared", 
                    _currentUserService.IPAddress, 
                    null, 
                    null, 
                    cancellationToken
                );
            }
            
            return Result.Success("Cart cleared successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart: {ErrorMessage}", ex.Message);
            return Result.Failure("Error clearing cart", ErrorCodeEnum.InternalError);
        }
    }
}