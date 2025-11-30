using MediatR;
using NekoViBE.Application.Common.DTOs.Shipping;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Shipping.Queries.GetShippingHistory;

public record GetShippingHistoryQuery(Guid OrderId) : IRequest<Result<List<ShippingHistoryDto>>>;

