using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore; // BẮT BUỘC ĐỂ DÙNG INCLUDE/THENINCLUDE
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Product.Queries.GetProduct
{
    public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Result<ProductResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductQueryHandler> _logger;
        private readonly IFileService _fileService;

        public GetProductQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetProductQueryHandler> logger, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<Result<ProductResponse>> Handle(GetProductQuery query, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            // 1. Tự build query để dùng được ThenInclude (Thay vì dùng GetFirstOrDefaultAsync của Repo)
            var dbQuery = _unitOfWork.Repository<Domain.Entities.Product>().GetQueryable();

            // 2. [QUAN TRỌNG] Include sâu (Chain Include)
            var entity = await dbQuery
                .Include(x => x.ProductImages)
                .Include(x => x.ProductTags).ThenInclude(pt => pt.Tag) // Lấy luôn Tag Name
                .Include(x => x.ProductReviews).ThenInclude(pr => pr.User) // Lấy luôn User Name
                .Include(x => x.OrderItems) // Để tính TotalSales
                .Include(x => x.Category)
                .Include(x => x.AnimeSeries)
                // Include EventProduct -> ThenInclude Event (Để tính Discount)
                .Include(x => x.EventProducts).ThenInclude(ep => ep.Event)
                .AsNoTracking() // Tối ưu hiệu năng Read-only
                .FirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted, cancellationToken);

            if (entity == null)
                return Result<ProductResponse>.Failure("Product not found", ErrorCodeEnum.NotFound);

            // 3. Mapping
            var response = _mapper.Map<ProductResponse>(entity);

            // 4. Xử lý đường dẫn ảnh (Full URL)
            if (response.Images != null)
            {
                foreach (var img in response.Images)
                {
                    img.ImagePath = _fileService.GetFileUrl(img.ImagePath);
                }
            }

            if (response.Events != null)
            {
                foreach (var eventItem in response.Events)
                {
                    eventItem.ImagePath = _fileService.GetFileUrl(eventItem.ImagePath);
                }
            }
            

            // 5. Tính toán Total Sales
            response.TotalSales = entity.OrderItems
                .Where(oi => !oi.IsDeleted)
                .Sum(oi => oi.Quantity);

            // 6. Tính toán Rating
            var reviews = entity.ProductReviews.Where(r => !r.IsDeleted).ToList();
            response.AverageRating = reviews.Any()
                ? Math.Round(reviews.Average(r => r.Rating), 2)
                : 0.0;

            // 7. [QUAN TRỌNG] Tìm Active Event để tính Discount
            // Vì đã dùng ThenInclude ở trên, ep.Event bây giờ chắc chắn có dữ liệu
            var activeEventProduct = entity.EventProducts
                .Where(ep => !ep.IsDeleted &&
                             ep.Event != null &&
                             !ep.Event.IsDeleted &&
                             ep.Event.Status == EntityStatusEnum.Active && // Check status Active
                             ep.Event.StartDate <= now &&
                             ep.Event.EndDate >= now)
                .OrderByDescending(ep => ep.DiscountPercentage) // Ưu tiên giảm giá cao nhất
                .FirstOrDefault();

            if (activeEventProduct != null)
            {
                response.ActiveEventDiscountPercentage = activeEventProduct.DiscountPercentage;
                response.EventDiscountPercentage = activeEventProduct.DiscountPercentage;
                response.ActiveEvent = _mapper.Map<EventItem>(activeEventProduct.Event);

                if (response.ActiveEvent != null && !string.IsNullOrEmpty(activeEventProduct.Event.ImagePath))
                {
                    response.ActiveEvent.ImagePath = _fileService.GetFileUrl(activeEventProduct.Event.ImagePath);
                }

                // (Tùy chọn) Tính luôn giá hiển thị DiscountPrice nếu DTO có trường này
                // response.DiscountPrice = entity.Price * (1 - activeEventProduct.DiscountPercentage / 100m);
            }
            else
            {
                response.ActiveEventDiscountPercentage = null;
                response.ActiveEvent = null;
            }

            return Result<ProductResponse>.Success(response);
        }
    }
}