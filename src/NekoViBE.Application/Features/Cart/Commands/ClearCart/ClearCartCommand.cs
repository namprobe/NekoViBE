using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand() : IRequest<Result>;