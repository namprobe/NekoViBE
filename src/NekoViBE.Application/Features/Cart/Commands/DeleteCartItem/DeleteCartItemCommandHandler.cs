using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Features.Cart.Commands.DeleteCartItem;

public class DeleteCartItemCommandHandler : IRequestHandler<DeleteCartItemCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteCartItemCommandHandler> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public DeleteCartItemCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<DeleteCartItemCommandHandler> logger, IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> Handle(DeleteCartItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }
            var isCartExist = await _unitOfWork.Repository<ShoppingCart>().AnyAsync(c => c.UserId == userId);
            if (!isCartExist)
                return Result.Failure("Cart not found", ErrorCodeEnum.NotFound);
            var cartItem = await _unitOfWork.Repository<CartItem>().GetFirstOrDefaultAsync(ci => ci.Id == command.CartItemId);
            if (cartItem == null)
                return Result.Failure("Cart item not found", ErrorCodeEnum.NotFound);
            _unitOfWork.Repository<CartItem>().Delete(cartItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //log user action (fire and forget)
            UserActionHelper.LogDeleteAction<CartItem>(_serviceProvider, userId.Value, cartItem.Id, _currentUserService.IPAddress, cancellationToken);
            return Result.Success("Cart item deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cart item: {ErrorMessage}", ex.Message);
            return Result.Failure("Error deleting cart item", ErrorCodeEnum.InternalError);
        }
    }
}