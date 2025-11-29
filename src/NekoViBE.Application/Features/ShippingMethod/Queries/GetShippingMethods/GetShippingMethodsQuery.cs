using MediatR;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.ShippingMethod.Queries.GetShippingMethods;

public record GetShippingMethodsQuery(ShippingMethodFilter Filter) : IRequest<PaginationResult<ShippingMethodItem>>;

