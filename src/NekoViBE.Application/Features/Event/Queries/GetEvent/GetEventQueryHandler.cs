using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Event;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
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

                // Với mỗi Product trong Event (Đoạn này load data chi tiết product)
                foreach (var ep in entity.EventProducts)
                {
                    ep.Product = await _unitOfWork.Repository<Domain.Entities.Product>()
                        .GetFirstOrDefaultAsync(
                            p => p.Id == ep.ProductId && !p.IsDeleted,
                            includes: new Expression<Func<Domain.Entities.Product, object>>[]
                            {
                                p => p.ProductImages,
                                p => p.Category,
                                p => p.ProductReviews
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
                    // Map Entity Product sang DTO ProductItem
                    var productDto = _mapper.Map<ProductItem>(ep.Product);

                    // --- [MODIFICATION START] --- 
                    // Gán DiscountPercentage từ EventProduct (bảng trung gian) vào ProductItem
                    productDto.EventDiscountPercentage = ep.DiscountPercentage;
                    // --- [MODIFICATION END] ---

                    // --- LOGIC TÍNH AVERAGE RATING ---
                    if (ep.Product.ProductReviews != null && ep.Product.ProductReviews.Any())
                    {
                        productDto.AverageRating = Math.Round(ep.Product.ProductReviews.Average(r => r.Rating), 1);
                        productDto.ReviewCount = ep.Product.ProductReviews.Count;
                    }
                    else
                    {
                        productDto.AverageRating = null;
                        productDto.ReviewCount = 0;
                    }

                    productDtos.Add(productDto);
                }

                // Gán danh sách đã xử lý vào response
                response.Products = productDtos;

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