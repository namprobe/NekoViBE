using MediatR;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.Models.GHN;

namespace NekoViBE.Application.Features.Shipping.Commands;

public record ProcessGHNCallbackCommand(GHNCallbackRequest Request, string? CallerIpAddress = null) : IRequest<Result<object>>;

