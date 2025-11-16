using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Product.Queries.GetProductList
{
    public class GetProductListQueryHandler : IRequestHandler<GetProductListQuery, PaginationResult<ProductItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public GetProductListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetProductListQueryHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<PaginationResult<ProductItem>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
        {

            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var isAscending = request.Filter.SortType?.EndsWith("asc") ?? false;


            var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.Product>().GetPagedAsync(
                pageNumber: request.Filter.Page,
                pageSize: request.Filter.PageSize,
                predicate: predicate,
                orderBy: orderBy,
                isAscending: isAscending,
                includes: new Expression<Func<Domain.Entities.Product, object>>[]
                {
                    x => x.ProductImages,
                    x => x.Category,
                    x => x.ProductTags
                });

            var productItems = _mapper.Map<List<ProductItem>>(items);

            // Gán URL đầy đủ cho ảnh chính (hoặc fallback nếu không có)
            foreach (var (product, entity) in productItems.Zip(items))
            {
                var primaryImage = entity.ProductImages.FirstOrDefault(img => img.IsPrimary);
                    product.PrimaryImage = _fileService.GetFileUrl(primaryImage.ImagePath);
                
            }

            // Log nếu có sản phẩm chưa có ảnh chính
            var productsWithoutPrimaryImage = items
                .Where(x => !x.ProductImages.Any(img => img.IsPrimary))
                .Select(x => x.Name)
                .ToList();


            return PaginationResult<ProductItem>.Success(
                productItems,
                request.Filter.Page,
                request.Filter.PageSize,
                totalCount);
        }
    }
}
