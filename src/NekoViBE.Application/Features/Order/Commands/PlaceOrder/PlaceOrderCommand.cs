using MediatR;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Order.Commands.PlaceOrder;

public record PlaceOrderCommand(PlaceOrderRequest Request) : IRequest<Result<object>>;