using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeries;

public class GetAnimeSeriesQueryHandler : IRequestHandler<GetAnimeSeriesQuery, Result<AnimeSeriesResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAnimeSeriesQueryHandler> _logger;

    public GetAnimeSeriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAnimeSeriesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AnimeSeriesResponse>> Handle(GetAnimeSeriesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _unitOfWork.Repository<Domain.Entities.AnimeSeries>().GetFirstOrDefaultAsync(x => x.Id == query.Id);

            if (entity == null)
                return Result<AnimeSeriesResponse>.Failure("Anime series not found", ErrorCodeEnum.NotFound);

            if (string.IsNullOrEmpty(entity.ImagePath))
                _logger.LogWarning("Anime series with ID {Id} has no ImagePath", query.Id);

            var response = _mapper.Map<AnimeSeriesResponse>(entity);
            return Result<AnimeSeriesResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting anime series with ID {Id}", query.Id);
            return Result<AnimeSeriesResponse>.Failure("Error getting anime series", ErrorCodeEnum.InternalError);
        }
    }
}