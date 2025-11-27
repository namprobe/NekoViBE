using MediatR;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Wishlist.Commands.AddToWishlist;

public class AddToWishlistCommand : IRequest<Result<bool>>
{
    public Guid ProductId { get; set; }
}
