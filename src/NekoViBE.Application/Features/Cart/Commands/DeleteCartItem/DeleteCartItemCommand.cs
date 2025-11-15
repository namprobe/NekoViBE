using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Cart.Commands.DeleteCartItem;

public record DeleteCartItemCommand(Guid CartItemId) : IRequest<Result>;
