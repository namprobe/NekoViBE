using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ShippingMethod.Commands.DeleteShippingMethod;

public record DeleteShippingMethodCommand(Guid Id) : IRequest<Result>;
