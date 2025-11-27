using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Wishlist.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommand : IRequest<Result<bool>>
{
    public Guid ProductId { get; set; }
}
