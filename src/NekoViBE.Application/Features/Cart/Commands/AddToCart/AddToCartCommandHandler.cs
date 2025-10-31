using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Features.Cart.Commands.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddToCartCommandHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;

    public AddToCartCommandHandler(
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<AddToCartCommandHandler> logger,
        IMapper mapper,
        IServiceProvider serviceProvider)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> Handle(AddToCartCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Implementation logic to add item to cart goes here.
            // This is a placeholder implementation.
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return Result.Failure("Invalid user.", ErrorCodeEnum.Unauthorized);
            }
            var cart = await _unitOfWork.Repository<ShoppingCart>().GetFirstOrDefaultAsync(c => c.UserId == userId.Value);
            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId.Value
                };
                cart.InitializeEntity(userId);
                await _unitOfWork.Repository<ShoppingCart>().AddAsync(cart);
            }
            var request = command.Request;
            //check product availability
            var product = await _unitOfWork.Repository<Domain.Entities.Product>().GetFirstOrDefaultAsync(p => p.Id == request.ProductId);
            if (product == null || product.Status != Domain.Enums.EntityStatusEnum.Active)
            {
                return Result.Failure("Product is not available.", ErrorCodeEnum.ValidationFailed);
            }
            //check stock availability
            if (product.StockQuantity < request.Quantity)
            {
                return Result.Failure("Insufficient stock for the product.", ErrorCodeEnum.ValidationFailed);
            }
            //check if the product is already in the cart
            var cartItem = await _unitOfWork.Repository<CartItem>().GetFirstOrDefaultAsync(ci => ci.ProductId == request.ProductId && ci.CartId == cart.Id);
            if (cartItem != null)
            {
                cartItem.Quantity += request.Quantity;
                cartItem.UpdateEntity(userId);
                _unitOfWork.Repository<CartItem>().Update(cartItem);
            }
            else
            {
                // Map the command to a domain entity
                cartItem = _mapper.Map<CartItem>(request);
                cartItem.CartId = cart.Id;
                cartItem.InitializeEntity(userId);
                // Add the item to the cart
                await _unitOfWork.Repository<CartItem>().AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();
            
            UserActionHelper.LogCreateAction(
                _serviceProvider,
                userId.Value,
                cartItem.Id,
                cartItem,
                _currentUserService.IPAddress,
                cancellationToken
            );

            return Result.Success("Item added to cart successfully.");
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            _logger.LogError(ex, "An error occurred while adding the item to the cart.");
            return Result.Failure("An error occurred while adding the item to the cart.", ErrorCodeEnum.InternalError);
        }
    }
}