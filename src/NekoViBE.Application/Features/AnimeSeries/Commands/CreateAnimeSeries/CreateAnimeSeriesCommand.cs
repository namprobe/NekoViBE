using MediatR;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.CreateAnimeSeries;

public record CreateAnimeSeriesCommand(AnimeSeriesRequest Request) : IRequest<Result>;
