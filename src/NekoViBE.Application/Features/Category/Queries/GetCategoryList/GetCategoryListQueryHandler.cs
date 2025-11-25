using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Category.Queries.GetCategoryList
{
    public class GetCategoryListQueryHandler : IRequestHandler<GetCategoryListQuery, PaginationResult<CategoryItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCategoryListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;

        public GetCategoryListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCategoryListQueryHandler> logger,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }

        public async Task<PaginationResult<CategoryItem>> Handle(GetCategoryListQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.Category>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending
                );

                var categoryItems = _mapper.Map<List<CategoryItem>>(items);

                foreach (var (category, entity) in categoryItems.Zip(items))
                {
                    category.ImagePath = _fileService.GetFileUrl(entity.ImagePath);
                }

                if (categoryItems.Any(item => string.IsNullOrEmpty(item.ImagePath)))
                    _logger.LogWarning("Some categories in the list have no ImagePath: {Names}",
                        string.Join(", ", items.Where(x => string.IsNullOrEmpty(x.ImagePath)).Select(x => x.Name)));

                return PaginationResult<CategoryItem>.Success(
                    categoryItems,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category list with filter: {@Filter}", request.Filter);
                return PaginationResult<CategoryItem>.Failure("Error getting category list", ErrorCodeEnum.InternalError);
            }
        }
    }
}
