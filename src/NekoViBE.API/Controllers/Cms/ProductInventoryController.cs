using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.ProductInventory;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.ProductInventory.Commands.CreateProductInventory;
using NekoViBE.Application.Features.ProductInventory.Commands.DeleteProductInventory;
using NekoViBE.Application.Features.ProductInventory.Commands.UpdateProductInventory;
using NekoViBE.Application.Features.ProductInventory.Queries.GetProductInventory;
using NekoViBE.Application.Features.ProductInventory.Queries.GetProductInventoryList;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;

namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/product-inventory")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_ProductInventory")]
    [SwaggerTag("This API is used for Product Inventory management in CMS")]
    public class ProductInventoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductInventoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(PaginationResult<ProductInventoryItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<ProductInventoryItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<ProductInventoryItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<ProductInventoryItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all product inventories with pagination and filtering",
            Description = "This API retrieves a paginated list of product inventories with filtering options",
            OperationId = "GetProductInventoryList",
            Tags = new[] { "CMS", "CMS_ProductInventory" }
        )]
        public async Task<IActionResult> GetProductInventoryList([FromQuery] ProductInventoryFilter filter)
        {
            var query = new GetProductInventoryListQuery(filter);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result<ProductInventoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<ProductInventoryResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<ProductInventoryResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<ProductInventoryResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<ProductInventoryResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get product inventory by ID",
            Description = "This API retrieves a specific product inventory by its ID",
            OperationId = "GetProductInventory",
            Tags = new[] { "CMS", "CMS_ProductInventory" }
        )]
        public async Task<IActionResult> GetProductInventory(Guid id)
        {
            var query = new GetProductInventoryQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }

        [HttpPost]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create a new product inventory",
            Description = "This API creates a new product inventory and updates the product's stock quantity. It requires Admin or Staff role access",
            OperationId = "CreateProductInventory",
            Tags = new[] { "CMS", "CMS_ProductInventory" }
        )]
        public async Task<IActionResult> CreateProductInventory([FromForm] ProductInventoryRequest request)
        {
            var command = new CreateProductInventoryCommand(request);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }

        
        [HttpPut("{id}")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Update an existing product inventory",
            Description = "This API updates an existing product inventory and adjusts the product's stock quantity. It requires Admin or Staff role access",
            OperationId = "UpdateProductInventory",
            Tags = new[] { "CMS", "CMS_ProductInventory" }
        )]
        public async Task<IActionResult> UpdateProductInventory(Guid id, [FromForm] ProductInventoryRequest request)
        {
            var command = new UpdateProductInventoryCommand(id, request);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete a product inventory",
            Description = "This API soft-deletes a product inventory and updates the product's stock quantity. It requires Admin or Staff role access",
            OperationId = "DeleteProductInventory",
            Tags = new[] { "CMS", "CMS_ProductInventory" }
        )]
        public async Task<IActionResult> DeleteProductInventory(Guid id)
        {
            var command = new DeleteProductInventoryCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }
    }
}
