using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.Wishlist;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Wishlist.Queries.GetWishlist;

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, Result<GetWishlistResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;

    public GetWishlistQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper,
        IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _fileService = fileService;
    }

    public async Task<Result<GetWishlistResponse>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Result<GetWishlistResponse>.Failure("User not authenticated", Common.Enums.ErrorCodeEnum.Unauthorized);

        // Lấy wishlist của user
        var wishlist = await _unitOfWork.Repository<Domain.Entities.Wishlist>()
            .GetQueryable()
            .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.Product)
                    .ThenInclude(p => p.ProductImages)
            .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.Product)
                    .ThenInclude(p => p.Category)
            .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.Product)
                    .ThenInclude(p => p.AnimeSeries)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        // Nếu chưa có wishlist -> trả về danh sách rỗng
        if (wishlist == null)
        {
            return Result<GetWishlistResponse>.Success(new GetWishlistResponse
            {
                WishlistId = Guid.Empty,
                Items = new List<WishlistItemDto>()
            });
        }

        var items = _mapper.Map<List<WishlistItemDto>>(wishlist.WishlistItems.OrderByDescending(x => x.CreatedAt));

        // Gán URL đầy đủ cho ảnh chính của mỗi sản phẩm
        foreach (var item in items)
        {
            var productEntity = wishlist.WishlistItems
                .FirstOrDefault(wi => wi.Id == item.WishlistItemId)?.Product;

            if (productEntity != null)
            {
                var primaryImage = productEntity.ProductImages?.FirstOrDefault(img => img.IsPrimary);
                if (primaryImage != null && item.Product != null)
                {
                    item.Product.PrimaryImage = _fileService.GetFileUrl(primaryImage.ImagePath);
                }
            }
        }

        var response = new GetWishlistResponse
        {
            WishlistId = wishlist.Id,
            Items = items
        };

        return Result<GetWishlistResponse>.Success(response);
    }
}
