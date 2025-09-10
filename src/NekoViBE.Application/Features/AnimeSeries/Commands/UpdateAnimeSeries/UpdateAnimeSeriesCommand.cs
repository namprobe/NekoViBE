using MediatR;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.UpdateAnimeSeries;

public record UpdateAnimeSeriesCommand(Guid Id, AnimeSeriesRequest Request) : IRequest<Result>;
