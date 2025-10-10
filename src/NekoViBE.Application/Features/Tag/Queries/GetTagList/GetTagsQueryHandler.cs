using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Tag.Queries.GetTagList
{
    public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, PaginationResult<TagItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTagsQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetTagsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetTagsQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<TagItem>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var isValid = (await _currentUserService.IsUserValidAsync()).isValid;
                if (!isValid)
                {
                    return PaginationResult<TagItem>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.Tag>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending
                );

                var tagItems = _mapper.Map<List<TagItem>>(items);

                return PaginationResult<TagItem>.Success(
                    tagItems,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách Tags với bộ lọc: {@Filter}", request.Filter);
                return PaginationResult<TagItem>.Failure(
                    "Error retrieving Tags",
                    ErrorCodeEnum.InternalError);
            }
        }
    }
}
