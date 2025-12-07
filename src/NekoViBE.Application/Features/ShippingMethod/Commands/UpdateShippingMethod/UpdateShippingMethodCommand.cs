using MediatR;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ShippingMethod.Commands.UpdateShippingMethod;

public record UpdateShippingMethodCommand(Guid Id, ShippingMethodRequest Request) : IRequest<Result>;
