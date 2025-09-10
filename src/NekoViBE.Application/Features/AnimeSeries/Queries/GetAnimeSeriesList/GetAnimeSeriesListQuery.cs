using MediatR;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeriesList;

public record GetAnimeSeriesListQuery(AnimeSeriesFilter Filter) : IRequest<PaginationResult<AnimeSeriesItem>>;
