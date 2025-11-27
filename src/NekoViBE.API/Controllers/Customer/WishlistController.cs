using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.DTOs.Wishlist;
using NekoViBE.Application.Features.Wishlist.Commands.AddToWishlist;
using NekoViBE.Application.Features.Wishlist.Commands.RemoveFromWishlist;
using NekoViBE.Application.Features.Wishlist.Queries.GetWishlist;

namespace NekoViBE.API.Controllers.Customer;

[Authorize]
[Route("api/customer/wishlist")]
[ApiController]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishlistController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách wishlist của user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWishlist(CancellationToken cancellationToken)
    {
        var query = new GetWishlistQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Thêm hoặc xóa sản phẩm khỏi wishlist (Toggle)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddToWishlist(
        [FromBody] AddToWishlistRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddToWishlistCommand
        {
            ProductId = request.ProductId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Xóa sản phẩm khỏi wishlist
    /// </summary>
    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFromWishlist(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveFromWishlistCommand
        {
            ProductId = productId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
