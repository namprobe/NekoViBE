using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Features.Wishlist.Commands.AddToWishlist;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddToWishlistCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Result<bool>.Failure("User not authenticated", ErrorCodeEnum.Unauthorized);

        // Check product tồn tại
        var product = await _unitOfWork.Repository<Domain.Entities.Product>()
            .GetByIdAsync(request.ProductId);
        
        if (product == null)
            return Result<bool>.Failure("Product not found", ErrorCodeEnum.NotFound);

        // Lazy Create: Lấy hoặc tạo wishlist
        var wishlist = await _unitOfWork.Repository<Domain.Entities.Wishlist>()
            .GetQueryable()
            .Include(w => w.WishlistItems)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wishlist == null)
        {
            wishlist = new Domain.Entities.Wishlist
            {
                UserId = userId,
                Name = "My Wishlist"
            };
            await _unitOfWork.Repository<Domain.Entities.Wishlist>().AddAsync(wishlist);
        }

        // Toggle logic: Nếu đã có -> xóa, chưa có -> thêm
        var existingItem = wishlist.WishlistItems
            .FirstOrDefault(wi => wi.ProductId == request.ProductId);

        if (existingItem != null)
        {
            // Xóa khỏi wishlist
            _unitOfWork.Repository<WishlistItem>().Delete(existingItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(false, "Removed from wishlist");
        }
        else
        {
            // Thêm vào wishlist
            var newItem = new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = request.ProductId
            };
            await _unitOfWork.Repository<WishlistItem>().AddAsync(newItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true, "Added to wishlist");
        }
    }
}
