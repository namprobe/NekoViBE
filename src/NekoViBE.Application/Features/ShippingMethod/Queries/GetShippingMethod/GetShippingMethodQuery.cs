using MediatR;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.DTOs.ShippingMethod;

namespace NekoViBE.Application.Features.ShippingMethod.Queries.GetShippingMethod;

public record GetShippingMethodQuery(Guid Id) : IRequest<Result<ShippingMethodResponse>>;

