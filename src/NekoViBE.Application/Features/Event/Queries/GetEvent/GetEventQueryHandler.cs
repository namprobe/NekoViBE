using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic; // Added for List<>
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Event.Queries.GetEvent
{
    public class GetEventQueryHandler : IRequestHandler<GetEventQuery, Result<EventResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEventQueryHandler> _logger;
        private readonly IFileService _fileService;

        public GetEventQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetEventQueryHandler> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<Result<EventResponse>> Handle(GetEventQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // Load Event với các Product liên quan
                var entity = await _unitOfWork.Repository<Domain.Entities.Event>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Id == query.Id && !x.IsDeleted,
                        includes: new Expression<Func<Domain.Entities.Event, object>>[]
                        {
                            x => x.EventProducts
                        });


                if (entity == null)
                {
                    return Result<EventResponse>.Failure("Event not found", ErrorCodeEnum.NotFound);
                }

                // Map sang DTO
                var response = _mapper.Map<EventResponse>(entity);

                // Gán URL đầy đủ cho ảnh của Event
                if (!string.IsNullOrEmpty(response.ImagePath))
                {
                    response.ImagePath = _fileService.GetFileUrl(response.ImagePath);
                }

                // Với mỗi Product trong Event
                foreach (var ep in entity.EventProducts)
                {
                    ep.Product = await _unitOfWork.Repository<Domain.Entities.Product>()
                        .GetFirstOrDefaultAsync(
                            p => p.Id == ep.ProductId && !p.IsDeleted,
                            includes: new Expression<Func<Domain.Entities.Product, object>>[]
                            {
                                p => p.ProductImages,
                                p => p.Category,
                                p => p.ProductReviews // <--- THÊM: Include ProductReviews để tính rating
                            });

                    if (ep.Product != null)
                    {
                        // convert image path sang URL
                        foreach (var img in ep.Product.ProductImages.Where(pi => !pi.IsDeleted))
                        {
                            img.ImagePath = _fileService.GetFileUrl(img.ImagePath);
                        }
                    }
                }

                // Xử lý mapping và tính toán Rating
                var productDtos = new List<ProductItem>();
                var validEventProducts = entity.EventProducts.Where(ep => ep.Product != null && !ep.Product.IsDeleted);

                foreach (var ep in validEventProducts)
                {
                    // Map Entity sang DTO
                    var productDto = _mapper.Map<ProductItem>(ep.Product);

                    // --- LOGIC TÍNH AVERAGE RATING (Giống GetProductList) ---
                    if (ep.Product.ProductReviews != null && ep.Product.ProductReviews.Any())
                    {
                        // Tính trung bình cộng rating, làm tròn 1 chữ số thập phân
                        productDto.AverageRating = Math.Round(ep.Product.ProductReviews.Average(r => r.Rating), 1);
                        productDto.ReviewCount = ep.Product.ProductReviews.Count;
                    }
                    else
                    {
                        productDto.AverageRating = null;
                        productDto.ReviewCount = 0;
                    }
                    // --------------------------------------------------------

                    productDtos.Add(productDto);
                }

                // Gán danh sách đã xử lý vào response
                response.Products = productDtos;

                // Log thông tin
                _logger.LogInformation("Retrieved Event {Id} with {ProductCount} products", query.Id, response.Products.Count);

                return Result<EventResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event with ID {Id}", query.Id);
                return Result<EventResponse>.Failure("Error getting event", ErrorCodeEnum.InternalError);
            }
        }
    }
}