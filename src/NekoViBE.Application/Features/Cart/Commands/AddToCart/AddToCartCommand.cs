using MediatR;
using NekoViBE.Application.Common.DTOs.Cart;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Cart.Commands.AddToCart;

public record AddToCartCommand(CartItemRequest Request) : IRequest<Result>;