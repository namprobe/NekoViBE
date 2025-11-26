using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Cart;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Features.Cart.Queries.GetCurrentUserCart;

public class GetCurrentUserCartQueryHandler : IRequestHandler<GetCurrentUserCartQuery, Result<CartResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetCurrentUserCartQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly string baseStorageUrl;
    public GetCurrentUserCartQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<GetCurrentUserCartQueryHandler> logger, IMapper mapper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _mapper = mapper;
        baseStorageUrl = configuration.GetValue<string>("FileStorage:BaseStorageUrl") ?? string.Empty;
        if (string.IsNullOrEmpty(baseStorageUrl))
        {
            throw new InvalidOperationException("BaseStorageUrl is not configured");
        }
    }
    public async Task<Result<CartResponse>> Handle(GetCurrentUserCartQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid || userId is null)
            {
                return Result<CartResponse>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
            }
            var cart = await _unitOfWork.Repository<ShoppingCart>().GetFirstOrDefaultAsync(c => c.UserId == userId);
            if (cart is null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId.Value
                };
                cart.InitializeEntity();
                await _unitOfWork.Repository<ShoppingCart>().AddAsync(cart);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<CartResponse>.Success(new CartResponse
                {
                    TotalItems = 0,
                    TotalPrice = 0,
                    CartItems = new List<CartItemResponse>()
                });
            }
            var filter = query.Filter;
            // get paged cart items - always sort by newest (CreatedAt descending)
            var (cartItems, totalCount) = await _unitOfWork.Repository<CartItem>().GetPagedAsync(
                pageNumber: filter.Page,
                pageSize: filter.PageSize,
                predicate: c => c.CartId == cart.Id,
                orderBy: c => c.CreatedAt!,
                isAscending: false, // newest first
                includes: c => c.Product);
            var totalPrice = await _unitOfWork.Repository<CartItem>().GetQueryable()
            .Where(c => c.CartId == cart.Id)
            .SumAsync(c => c.Product.Price * c.Quantity);
            
            // Tối ưu: Load tất cả primary images trong 1 query duy nhất
            var productIds = cartItems.Select(c => c.ProductId).ToList();
            var primaryImages = await _unitOfWork.Repository<Domain.Entities.ProductImage>()
                .FindAsync(p => productIds.Contains(p.ProductId) && p.IsPrimary == true);
            var imageDict = primaryImages.ToDictionary(p => p.ProductId, p => p.ImagePath);
            
            var cartResponse = _mapper.Map<CartResponse>(cart);
            cartResponse.TotalItems = totalCount;
            cartResponse.TotalPrice = totalPrice;
            
            // Map cart items với images từ dictionary (không cần async)
            cartResponse.CartItems = cartItems.Select(c => {
                var cartItemResponse = _mapper.Map<CartItemResponse>(c);
                cartItemResponse.ImagePath = imageDict.TryGetValue(c.ProductId, out var imagePath)
                    ? $"{baseStorageUrl}{imagePath}" 
                    : string.Empty;
                return cartItemResponse;
            }).ToList();
            return Result<CartResponse>.Success(cartResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user cart");
            return Result<CartResponse>.Failure("Error getting current user cart", ErrorCodeEnum.InternalError);
        }
    }
}