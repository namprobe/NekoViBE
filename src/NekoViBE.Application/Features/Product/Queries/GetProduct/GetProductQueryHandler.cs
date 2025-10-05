using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
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
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.Product>().GetFirstOrDefaultAsync(
                    predicate: x => x.Id == query.Id,
                    includes: new Expression<Func<Domain.Entities.Product, object>>[]
                    {
                        x => x.ProductImages,
                        x => x.ProductTags,
                        x => x.ProductReviews,
                        x => x.EventProducts,
                        x => x.OrderItems,
                        x => x.Category,
                        x => x.AnimeSeries
                    });

                if (entity == null)
                    return Result<ProductResponse>.Failure("Product not found", ErrorCodeEnum.NotFound);

                // Load Tags for each ProductTag
                foreach (var pt in entity.ProductTags)
                {
                    pt.Tag = await _unitOfWork.Repository<Domain.Entities.Tag>()
                        .GetFirstOrDefaultAsync(t => t.Id == pt.TagId);
                }

                // Load Users for each ProductReview
                foreach (var pr in entity.ProductReviews)
                {
                    pr.User = await _unitOfWork.Repository<Domain.Entities.AppUser>()
                        .GetFirstOrDefaultAsync(t => t.Id == pr.UserId);
                }

                // Map to ProductResponse
                var response = _mapper.Map<ProductResponse>(entity);

                // Set full URL for ImagePath
                if (response.Images != null && response.Images.Any())
                {
                    foreach (var img in response.Images)
                    {
                        img.ImagePath = _fileService.GetFileUrl(img.ImagePath);
                    }
                }

                // Calculate TotalSales
                response.TotalSales = entity.OrderItems.Where(oi => !oi.IsDeleted).Sum(oi => oi.Quantity);
                _logger.LogInformation("Product {Id} has total sales: {TotalSales}", query.Id, response.TotalSales);

                // Calculate AverageRating
                var reviews = entity.ProductReviews.Where(r => !r.IsDeleted).ToList();
                if (reviews.Any())
                {
                    response.AverageRating = Math.Round(reviews.Average(r => r.Rating), 2);
                    _logger.LogInformation("Product {Id} has average rating: {AverageRating} from {ReviewCount} reviews",
                        query.Id, response.AverageRating, reviews.Count);
                }
                else
                {
                    response.AverageRating = 0.0;
                    _logger.LogInformation("Product {Id} has no reviews", query.Id);
                }

                // Log image paths
                var imagePaths = string.Join(", ", entity.ProductImages.Where(img => !img.IsDeleted).Select(img => img.ImagePath));

                return Result<ProductResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID {Id}", query.Id);
                return Result<ProductResponse>.Failure("Error getting product", ErrorCodeEnum.InternalError);
            }
        }
    }
}