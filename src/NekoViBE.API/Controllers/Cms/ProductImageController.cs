using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.ProductImage.Commands.CreateProductImage;
using NekoViBE.Application.Features.ProductImage.Commands.DeleteProductImage;
using NekoViBE.Application.Features.ProductImage.Commands.UpdateProductImage;
using NekoViBE.Application.Features.ProductImage.Queries.GetProductImage;
using NekoViBE.Application.Features.ProductImage.Queries.GetProductImageList;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;

namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/product-image")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_ProductImage")]
    [SwaggerTag("This API is used for Product Image management in CMS")]
    public class ProductImageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductImageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(PaginationResult<ProductImageItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<ProductImageItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<ProductImageItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<ProductImageItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all product images with pagination and filtering",
            Description = "This API retrieves a paginated list of product images with filtering options",
            OperationId = "GetProductImageList",
            Tags = new[] { "CMS", "CMS_ProductImage" }
        )]
        public async Task<IActionResult> GetProductImageList([FromQuery] ProductImageFilter filter)
        {
            var query = new GetProductImageListQuery(filter);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result<ProductImageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<ProductImageResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<ProductImageResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<ProductImageResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<ProductImageResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get product image by ID",
            Description = "This API retrieves a specific product image by its ID",
            OperationId = "GetProductImage",
            Tags = new[] { "CMS", "CMS_ProductImage" }
        )]
        public async Task<IActionResult> GetProductImage(Guid id)
        {
            var query = new GetProductImageQuery(id);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Create a new product image
        /// </summary>
        /// <remarks>
        /// This API creates a new product image and saves the uploaded image file.
        /// It requires Admin or Staff role access.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/cms/product-image
        ///     Content-Type: multipart/form-data
        ///     
        ///     productId = 123e4567-e89b-12d3-a456-426614174000
        ///     image = [file upload]
        ///     isPrimary = true
        ///     displayOrder = 1
        ///     status = 1
        /// 
        /// Headers:
        ///     Authorization: Bearer &lt;access_token&gt;
        /// </remarks>
        /// <param name="request">Product image creation request</param>
        /// <returns>Creation result</returns>
        /// <response code="200">Product image created successfully</response>
        /// <response code="400">Creation failed (validation error)</response>
        /// <response code="401">Failed to create product image (not authorized)</response>
        /// <response code="403">No access (user is not Admin or Staff)</response>
        /// <response code="404">Product not found</response>
        /// <response code="500">Failed to create product image (internal server error)</response>
        [HttpPost]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create a new product image",
            Description = "This API creates a new product image and saves the uploaded image file. It requires Admin or Staff role access",
            OperationId = "CreateProductImage",
            Tags = new[] { "CMS", "CMS_ProductImage" }
        )]
        public async Task<IActionResult> CreateProductImage([FromForm] ProductImageRequest request)
        {
            var command = new CreateProductImageCommand(request);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update an existing product image
        /// </summary>
        /// <remarks>
        /// This API updates an existing product image and optionally replaces the image file.
        /// It requires Admin or Staff role access.
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/cms/product-image/123e4567-e89b-12d3-a456-426614174000
        ///     Content-Type: multipart/form-data
        ///     
        ///     productId = 123e4567-e89b-12d3-a456-426614174000
        ///     image = [file upload, optional]
        ///     isPrimary = true
        ///     displayOrder = 2
        ///     status = 1
        /// 
        /// Headers:
        ///     Authorization: Bearer &lt;access_token&gt;
        /// </remarks>
        /// <param name="id">Product image ID</param>
        /// <param name="request">Product image update request</param>
        /// <returns>Update result</returns>
        /// <response code="200">Product image updated successfully</response>
        /// <response code="400">Update failed (validation error)</response>
        /// <response code="401">Failed to update product image (not authorized)</response>
        /// <response code="403">No access (user is not Admin or Staff)</response>
        /// <response code="404">Product image or product not found</response>
        /// <response code="500">Failed to update product image (internal server error)</response>
        [HttpPut("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Update an existing product image",
            Description = "This API updates an existing product image and optionally replaces the image file. It requires Admin or Staff role access",
            OperationId = "UpdateProductImage",
            Tags = new[] { "CMS", "CMS_ProductImage" }
        )]
        public async Task<IActionResult> UpdateProductImage(Guid id, [FromForm] ProductImageRequest request)
        {
            var command = new UpdateProductImageCommand(id, request);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete a product image",
            Description = "This API soft-deletes a product image and removes the associated file. It requires Admin or Staff role access",
            OperationId = "DeleteProductImage",
            Tags = new[] { "CMS", "CMS_ProductImage" }
        )]
        public async Task<IActionResult> DeleteProductImage(Guid id)
        {
            var command = new DeleteProductImageCommand(id);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return StatusCode(result.GetHttpStatusCode(), result);
            }
            return Ok(result);
        }
    }
}