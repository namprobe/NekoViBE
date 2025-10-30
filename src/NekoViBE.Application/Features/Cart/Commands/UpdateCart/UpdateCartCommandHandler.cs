using AutoMapper;
using MediatR;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Common;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Helpers;

namespace NekoViBE.Application.Features.Cart.Commands.UpdateCart;

public class UpdateCartCommandHandler : IRequestHandler<UpdateCartCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateCartCommandHandler> _logger;

    public UpdateCartCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IServiceProvider serviceProvider, ILogger<UpdateCartCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateCartCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }
            var cartItem = await _unitOfWork.Repository<CartItem>()
                .GetFirstOrDefaultAsync(ci => ci.Id == command.CartItemId);

            if (cartItem == null)
            {
                return Result.Failure("Cart item not found.", ErrorCodeEnum.NotFound);
            }

            if (command.Quantity == 0)
            {
                _unitOfWork.Repository<CartItem>().Delete(cartItem);
                await _unitOfWork.SaveChangesAsync();
                return Result.Success("Cart item removed successfully.");
            }

            if (cartItem.Quantity == command.Quantity)
            {
                return Result.Success("Quantity is the same. No changes made.");
            }

            cartItem.Quantity = command.Quantity;
            cartItem.UpdateEntity(userId);
            await _unitOfWork.SaveChangesAsync();

            // Log user action using helper (fire and forget)
            UserActionHelper.LogUpdateAction(
                _serviceProvider,
                userId.Value,
                cartItem.Id,
                null, // no old object snapshot in this trim-down version
                cartItem,
                _currentUserService.IPAddress,
                cancellationToken
            );

            return Result.Success("Cart item updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item with ID {CartItemId}", command.CartItemId);
            return Result.Failure($"An error occurred while updating the cart item", ErrorCodeEnum.InternalError);
        }
    }
}