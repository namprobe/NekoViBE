using MediatR;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.AnimeSeries.Queries.GetAnimeSeries;

public record GetAnimeSeriesQuery(Guid Id) : IRequest<Result<AnimeSeriesResponse>>;
