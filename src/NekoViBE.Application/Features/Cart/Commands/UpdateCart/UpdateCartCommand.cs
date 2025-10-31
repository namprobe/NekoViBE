using MediatR;
using NekoViBE.Application.Common.DTOs.Cart;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Cart.Commands.UpdateCart;

public record UpdateCartCommand(Guid CartItemId, int Quantity) : IRequest<Result>;