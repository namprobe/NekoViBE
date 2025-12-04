// File: Application/Features/EventProduct/Queries/GetEventProductsByEventId/GetEventProductsByEventIdQueryHandler.cs
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.EventProduct;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.EventProduct.Queries.GetEventProductsByEventId
{
    public class GetEventProductsByEventIdQueryHandler
        : IRequestHandler<GetEventProductsByEventIdQuery, Result<List<EventProductWithProductItem>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEventProductsByEventIdQueryHandler> _logger;

        public GetEventProductsByEventIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetEventProductsByEventIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<EventProductWithProductItem>>> Handle(
            GetEventProductsByEventIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var repo = _unitOfWork.Repository<Domain.Entities.EventProduct>();

                var items = await repo.GetQueryable()
                    .AsNoTracking()
                    .Where(ep => ep.EventId == request.EventId && !ep.IsDeleted)
                    .Include(ep => ep.Product)
                        .ThenInclude(p => p.Category)
                    .Include(ep => ep.Product)
                        .ThenInclude(p => p.AnimeSeries)
                    // Nếu bạn có Reviews để tính AverageRating thì include thêm ở đây
                    .OrderBy(ep => ep.Product.Name)
                    .ProjectTo<EventProductWithProductItem>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                // Nếu muốn tính discount price ngay trong event
                foreach (var item in items)
                {
                    if (item.DiscountPercentage > 0 && item.Product != null)
                    {
                        var discountAmount = item.Product.Price * item.DiscountPercentage / 100;
                        item.Product.DiscountPrice = item.Product.Price - discountAmount;
                        item.Product.EventDiscountPercentage = item.DiscountPercentage;
                    }
                }

                return Result<List<EventProductWithProductItem>>.Success(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting event products for EventId {EventId}", request.EventId);
                return Result<List<EventProductWithProductItem>>.Failure(
                    "Lỗi khi lấy danh sách sản phẩm của event",
                    ErrorCodeEnum.InternalError);
            }
        }
    }
}