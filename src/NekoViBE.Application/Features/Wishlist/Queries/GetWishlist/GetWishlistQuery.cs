using MediatR;
using NekoViBE.Application.Common.DTOs.Wishlist;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.Wishlist.Queries.GetWishlist;

public class GetWishlistQuery : IRequest<Result<GetWishlistResponse>>
{
}
