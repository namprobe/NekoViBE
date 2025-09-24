using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using System.Text.Json;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductImage.Queries.GetProductImageList
{
    public class GetProductImageListQueryHandler : IRequestHandler<GetProductImageListQuery, PaginationResult<ProductImageItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductImageListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetProductImageListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetProductImageListQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<ProductImageItem>> Handle(GetProductImageListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, _) = await _currentUserService.IsUserValidAsync();
                if (!isValid)
                {
                    return PaginationResult<ProductImageItem>.Failure(
                        "User is not valid",
                        ErrorCodeEnum.Unauthorized);
                }

                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.ProductImage>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: true
                );

                var productImageItems = _mapper.Map<List<ProductImageItem>>(items);

                return PaginationResult<ProductImageItem>.Success(
                    productImageItems,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product image list with filter: {@Filter}", request.Filter);
                return PaginationResult<ProductImageItem>.Failure(
                    "Error getting product image list",
                    ErrorCodeEnum.InternalError);
            }
        }
    }
}