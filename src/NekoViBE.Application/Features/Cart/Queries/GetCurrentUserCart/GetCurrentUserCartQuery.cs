using MediatR;
using NekoViBE.Application.Common.DTOs.Cart;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Cart.Queries.GetCurrentUserCart;

public record GetCurrentUserCartQuery(BasePaginationFilter Filter) : IRequest<Result<CartResponse>>;