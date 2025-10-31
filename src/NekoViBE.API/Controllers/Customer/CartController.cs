using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Cart;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Cart.Commands.AddToCart;
using NekoViBE.Application.Features.Cart.Commands.ClearCart;
using NekoViBE.Application.Features.Cart.Commands.DeleteCartItem;
using NekoViBE.Application.Features.Cart.Commands.UpdateCart;
using NekoViBE.Application.Features.Cart.Queries.GetCurrentUserCart;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer;

/// <summary>
/// Controller quản lý Shopping Cart cho Customer
/// </summary>
[ApiController]
[Route("api/customer/cart")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Customer", "Customer_Cart")]
[SwaggerTag("This API is used for Shopping Cart management by Customers")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user's shopping cart with pagination
    /// </summary>
    /// <remarks>
    /// This API retrieves the current user's shopping cart with pagination.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/customer/cart?page=1&amp;pageSize=10
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="filter">Pagination filter parameters</param>
    /// <returns>Shopping cart with items</returns>
    /// <response code="200">Cart retrieved successfully</response>
    /// <response code="401">Failed to retrieve cart (not authorized)</response>
    /// <response code="500">Failed to retrieve cart (internal server error)</response>
    [HttpGet]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result<CartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CartResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<CartResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get current user's shopping cart with pagination",
        Description = "This API retrieves the current user's shopping cart with pagination",
        OperationId = "Customer_GetCart",
        Tags = new[] { "Customer", "Customer_Cart" }
    )]
    public async Task<IActionResult> GetCart([FromQuery] BasePaginationFilter filter)
    {
        var query = new GetCurrentUserCartQuery(filter);
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Add a product to shopping cart
    /// </summary>
    /// <remarks>
    /// This API adds a product to the current user's shopping cart.
    /// If the product already exists in the cart, the quantity will be increased.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/customer/cart
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "productId": "123e4567-e89b-12d3-a456-426614174000",
    ///        "quantity": 2
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="request">Cart item request</param>
    /// <returns>Addition result</returns>
    /// <response code="200">Item added to cart successfully</response>
    /// <response code="400">Addition failed (validation error or insufficient stock)</response>
    /// <response code="401">Failed to add item (not authorized)</response>
    /// <response code="500">Failed to add item (internal server error)</response>
    [HttpPost]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Add a product to shopping cart",
        Description = "This API adds a product to the current user's shopping cart. If the product already exists, quantity will be increased",
        OperationId = "Customer_AddToCart",
        Tags = new[] { "Customer", "Customer_Cart" }
    )]
    public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request)
    {
        var command = new AddToCartCommand(request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    /// <remarks>
    /// This API updates the quantity of a cart item.
    /// **Note:** Setting quantity to 0 will delete the item from cart.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/customer/cart/123e4567-e89b-12d3-a456-426614174000
    ///     Content-Type: application/json
    ///     
    ///     {
    ///        "productId": "123e4567-e89b-12d3-a456-426614174000",
    ///        "quantity": 5
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="cartItemId">Cart item ID</param>
    /// <param name="request">Update request containing new quantity</param>
    /// <returns>Update result</returns>
    /// <response code="200">Cart item updated successfully</response>
    /// <response code="400">Update failed (validation error)</response>
    /// <response code="401">Failed to update cart item (not authorized)</response>
    /// <response code="404">Cart item not found</response>
    /// <response code="500">Failed to update cart item (internal server error)</response>
    [HttpPut("{cartItemId}")]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update cart item quantity",
        Description = "This API updates the quantity of a cart item. Setting quantity to 0 will delete the item",
        OperationId = "Customer_UpdateCartItem",
        Tags = new[] { "Customer", "Customer_Cart" }
    )]
    public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] UpdateCartItemRequest request)
    {
        var command = new UpdateCartCommand(cartItemId, request.Quantity);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Delete a cart item
    /// </summary>
    /// <remarks>
    /// This API removes a specific item from the shopping cart.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/customer/cart/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="cartItemId">Cart item ID</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Cart item deleted successfully</response>
    /// <response code="401">Failed to delete cart item (not authorized)</response>
    /// <response code="404">Cart item not found</response>
    /// <response code="500">Failed to delete cart item (internal server error)</response>
    [HttpDelete("{cartItemId}")]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete a cart item",
        Description = "This API removes a specific item from the shopping cart",
        OperationId = "Customer_DeleteCartItem",
        Tags = new[] { "Customer", "Customer_Cart" }
    )]
    public async Task<IActionResult> DeleteCartItem(Guid cartItemId)
    {
        var command = new DeleteCartItemCommand(cartItemId);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Clear all items from shopping cart
    /// </summary>
    /// <remarks>
    /// This API removes all items from the current user's shopping cart.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/customer/cart/clear
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <returns>Clear result</returns>
    /// <response code="200">Cart cleared successfully</response>
    /// <response code="401">Failed to clear cart (not authorized)</response>
    /// <response code="404">Cart not found</response>
    /// <response code="500">Failed to clear cart (internal server error)</response>
    [HttpDelete("clear")]
    [AuthorizeRoles("Customer")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Clear all items from shopping cart",
        Description = "This API removes all items from the current user's shopping cart",
        OperationId = "Customer_ClearCart",
        Tags = new[] { "Customer", "Customer_Cart" }
    )]
    public async Task<IActionResult> ClearCart()
    {
        var command = new ClearCartCommand();
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

/// <summary>
/// Request model for updating cart item quantity
/// </summary>
public class UpdateCartItemRequest
{
    /// <summary>
    /// New quantity (set to 0 to remove item)
    /// </summary>
    public int Quantity { get; set; }
}


