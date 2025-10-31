using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.AnimeSeries.Commands.DeleteAnimeSeries;

public record DeleteAnimeSeriesCommand(Guid Id) : IRequest<Result>;
