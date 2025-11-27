using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Features.Wishlist.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveFromWishlistCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Result<bool>.Failure("User not authenticated", ErrorCodeEnum.Unauthorized);

        var wishlist = await _unitOfWork.Repository<Domain.Entities.Wishlist>()
            .GetQueryable()
            .Include(w => w.WishlistItems)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wishlist == null)
            return Result<bool>.Failure("Wishlist not found", ErrorCodeEnum.NotFound);

        var item = wishlist.WishlistItems.FirstOrDefault(wi => wi.ProductId == request.ProductId);
        if (item == null)
            return Result<bool>.Failure("Item not in wishlist", ErrorCodeEnum.NotFound);

        _unitOfWork.Repository<WishlistItem>().Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, "Removed from wishlist");
    }
}
