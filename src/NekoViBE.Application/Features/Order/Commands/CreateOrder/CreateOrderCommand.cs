using MediatR;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Models;


namespace NekoViBE.Application.Features.Order.Commands.CreateOrder
{
    public record CreateOrderCommand(CreateOrderRequest Request) : IRequest<Result>;

}
