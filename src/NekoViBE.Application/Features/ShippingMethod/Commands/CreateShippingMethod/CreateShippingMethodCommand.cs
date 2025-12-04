using MediatR;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ShippingMethod.Commands.CreateShippingMethod;

public record CreateShippingMethodCommand(ShippingMethodRequest Request) : IRequest<Result>;
