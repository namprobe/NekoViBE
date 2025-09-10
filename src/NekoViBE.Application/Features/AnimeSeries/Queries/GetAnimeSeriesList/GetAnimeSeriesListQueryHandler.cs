using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;

namespace NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeriesList;

public class GetAnimeSeriesListQueryHandler
    : IRequestHandler<GetAnimeSeriesListQuery, PaginationResult<AnimeSeriesItem>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAnimeSeriesListQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public GetAnimeSeriesListQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetAnimeSeriesListQueryHandler> logger,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<PaginationResult<AnimeSeriesItem>> Handle(GetAnimeSeriesListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate user first
            var isValid = (await _currentUserService.IsUserValidAsync()).isValid;
            if (!isValid)
            {
                return PaginationResult<AnimeSeriesItem>.Failure(
                    "User is not valid",
                    ErrorCodeEnum.Unauthorized);
            }

            // Build query using extension methods
            var predicate = request.Filter.BuildPredicate();
            var orderBy = request.Filter.BuildOrderBy();
            var isAscending = request.Filter.IsAscending ?? false;

            // Query AnimeSeries from repository
            var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.AnimeSeries>().GetPagedAsync(
                pageNumber: request.Filter.Page,
                pageSize: request.Filter.PageSize,
                predicate: predicate,
                orderBy: orderBy,
                isAscending: isAscending
            );

            // Map to DTOs
            var animeSeriesItems = _mapper.Map<List<AnimeSeriesItem>>(items);

            return PaginationResult<AnimeSeriesItem>.Success(
                animeSeriesItems,
                request.Filter.Page,
                request.Filter.PageSize,
                totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting anime series list with filter: {@Filter}", request.Filter);
            return PaginationResult<AnimeSeriesItem>.Failure(
                "Error getting anime series list",
                ErrorCodeEnum.InternalError);
        }
    }
}
