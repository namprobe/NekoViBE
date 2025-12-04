using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore; // Bắt buộc để dùng Include/ThenInclude
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using NekoViBE.Domain.Enums; // Để dùng EntityStatusEnum

namespace NekoViBE.Application.Features.Product.Queries.GetProductList
{
    public class GetProductListQueryHandler : IRequestHandler<GetProductListQuery, PaginationResult<ProductItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductListQueryHandler> _logger;
        private readonly IFileService _fileService;

        public GetProductListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetProductListQueryHandler> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<PaginationResult<ProductItem>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            // 1. Khởi tạo Query từ GenericRepository (Đã có AsNoTracking)
            var query = _unitOfWork.Repository<Domain.Entities.Product>().GetQueryable();

            // 2. [QUAN TRỌNG] Sử dụng Include chuỗi (Chain) để lấy dữ liệu cấp 2
            query = query
                .Include(x => x.ProductImages)
                .Include(x => x.Category)
                .Include(x => x.ProductTags)
                .Include(x => x.ProductReviews)
                // Dùng ThenInclude để lấy thông tin Event từ bảng trung gian EventProducts
                .Include(x => x.EventProducts)
                    .ThenInclude(ep => ep.Event);

            // 3. Áp dụng Filter (Predicate)
            var predicate = request.Filter.BuildPredicate();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // 4. Áp dụng Sorting
            if (request.Filter.SortType?.StartsWith("updated") == true)
            {
                query = query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);
            }
            else
            {
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.GetIsAscending();
                if (orderBy != null)
                {
                    query = isAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
                }
            }

            // 5. Thực hiện Phân trang (Pagination) thủ công
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.Filter.Page - 1) * request.Filter.PageSize)
                .Take(request.Filter.PageSize)
                .ToListAsync(cancellationToken);

            // 6. Mapping & Tính toán Logic
            var productItems = _mapper.Map<List<ProductItem>>(items);

            // Zip để duyệt song song Entity và DTO
            foreach (var (dto, entity) in productItems.Zip(items, (d, e) => (d, e)))
            {
                // Xử lý ảnh đại diện
                var primaryImage = entity.ProductImages.FirstOrDefault(img => img.IsPrimary && !img.IsDeleted);
                dto.PrimaryImage = primaryImage != null
                    ? _fileService.GetFileUrl(primaryImage.ImagePath)
                    : null;

                // Tính Rating
                var reviews = entity.ProductReviews.Where(r => !r.IsDeleted).ToList();
                if (reviews.Any())
                {
                    dto.AverageRating = Math.Round(reviews.Average(r => r.Rating), 1);
                    dto.ReviewCount = reviews.Count;
                }

                // [LOGIC GIẢM GIÁ]
                // Tìm sự kiện đang diễn ra mà sản phẩm này tham gia
                var activeEventProduct = entity.EventProducts
                    .Where(ep =>
                        !ep.IsDeleted &&
                        ep.Event != null &&               // Kiểm tra Event đã được Load chưa (nhờ ThenInclude)
                        !ep.Event.IsDeleted &&
                        ep.Event.Status == EntityStatusEnum.Active && // Sự kiện phải đang Active
                        ep.Event.StartDate <= now &&      // Đã bắt đầu
                        ep.Event.EndDate >= now           // Chưa kết thúc
                    )
                    .OrderByDescending(ep => ep.DiscountPercentage) // Ưu tiên mức giảm cao nhất
                    .FirstOrDefault();

                if (activeEventProduct != null)
                {
                    dto.EventDiscountPercentage = activeEventProduct.DiscountPercentage;

                    // (Tùy chọn) Nếu bạn muốn hiển thị giá đã giảm luôn ở đây
                    // dto.DiscountPrice = entity.Price * (1 - activeEventProduct.DiscountPercentage / 100m);
                }
                else
                {
                    dto.EventDiscountPercentage = null;
                }
            }

            return PaginationResult<ProductItem>.Success(productItems, request.Filter.Page, request.Filter.PageSize, totalCount);
        }
    }
}