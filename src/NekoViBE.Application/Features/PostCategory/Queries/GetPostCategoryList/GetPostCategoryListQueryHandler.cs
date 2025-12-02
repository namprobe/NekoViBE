// NekoViBE.Application.Features.PostCategory.Queries.GetPostCategoryList/GetPostCategoryListQueryHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;

namespace NekoViBE.Application.Features.PostCategory.Queries.GetPostCategoryList;

public class GetPostCategoryListQueryHandler : IRequestHandler<GetPostCategoryListQuery, PaginationResult<PostCategoryItem>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPostCategoryListQueryHandler> _logger;

    public GetPostCategoryListQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetPostCategoryListQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginationResult<PostCategoryItem>> Handle(GetPostCategoryListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var isAscending = request.Filter.IsAscending ?? false;

            var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.PostCategory>().GetPagedAsync(
                pageNumber: request.Filter.Page,
                pageSize: request.Filter.PageSize,
                predicate: predicate,
                orderBy: orderBy,
                isAscending: isAscending
            );

            var postCategoryItems = _mapper.Map<List<PostCategoryItem>>(items);

            return PaginationResult<PostCategoryItem>.Success(
                postCategoryItems,
                request.Filter.Page,
                request.Filter.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post category list with filter: {@Filter}", request.Filter);
            return PaginationResult<PostCategoryItem>.Failure(
                "Error getting post category list",
                ErrorCodeEnum.InternalError);
        }
    }
}