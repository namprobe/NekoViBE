using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductTag.Queries.GetProductTagList
{
    public class GetProductTagsQueryHandler : IRequestHandler<GetProductTagsQuery, PaginationResult<ProductTagItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductTagsQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetProductTagsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetProductTagsQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<ProductTagItem>> Handle(GetProductTagsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var isValid = (await _currentUserService.IsUserValidAsync()).isValid;
                if (!isValid)
                {
                    return PaginationResult<ProductTagItem>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.ProductTag>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending
                );

                var productTagItems = _mapper.Map<List<ProductTagItem>>(items);

                return PaginationResult<ProductTagItem>.Success(
                    productTagItems,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách ProductTags với bộ lọc: {@Filter}", request.Filter);
                return PaginationResult<ProductTagItem>.Failure(
                    "Error retrieving ProductTags",
                    ErrorCodeEnum.InternalError);
            }
        }
    }
}
