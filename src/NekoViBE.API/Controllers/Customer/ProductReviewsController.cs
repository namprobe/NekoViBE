using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.ProductReview.Commands.CreateProductReview;
using NekoViBE.Application.Features.ProductReview.Commands.DeleteProductReview;
using NekoViBE.Application.Features.ProductReview.Commands.UpdateProductReview;
using NekoViBE.Application.Features.ProductReview.Queries.GetProductReview;
using NekoViBE.Application.Features.ProductReview.Queries.GetProductReviewList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/product-reviews")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_ProductReviews")]
    [SwaggerTag("This API is used for Product Review management in customer")]
    public class ProductReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductReviewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<ProductReviewItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<ProductReviewItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<ProductReviewItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<ProductReviewItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all product reviews with pagination and filtering",
            Description = "This API retrieves a paginated list of product reviews with filtering options",
            OperationId = "GetProductReviewList",
            Tags = new[] { "Customer", "Customer_ProductReviews" }
        )]
        public async Task<IActionResult> GetProductReviewList([FromQuery] ProductReviewFilter filter)
        {
            var query = new GetProductReviewListQuery(filter);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<ProductReviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<ProductReviewResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<ProductReviewResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<ProductReviewResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<ProductReviewResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get product review by ID",
            Description = "This API retrieves a specific product review by its ID",
            OperationId = "GetProductReview",
            Tags = new[] { "Customer", "Customer_ProductReviews" }
        )]
        public async Task<IActionResult> GetProductReview(Guid id)
        {
            var query = new GetProductReviewQuery(id);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPost]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Create a new product review",
            Description = "This API creates a new product review. Requires Admin, Staff, or User role access",
            OperationId = "CreateProductReview",
            Tags = new[] { "Customer", "Customer_ProductReviews" }
        )]
        public async Task<IActionResult> CreateProductReview([FromBody] ProductReviewRequest request)
        {
            var command = new CreateProductReviewCommand(request);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
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
            Summary = "Update an existing product review",
            Description = "This API updates an existing product review. Requires Admin, Staff, or the review's owner (User) role access",
            OperationId = "UpdateProductReview",
            Tags = new[] { "Customer", "Customer_ProductReviews" }
        )]
        public async Task<IActionResult> UpdateProductReview(Guid id, [FromBody] ProductReviewRequest request)
        {
            var command = new UpdateProductReviewCommand(id, request);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Delete a product review",
            Description = "This API deletes a product review. Requires Admin, Staff, or the review's owner (User) role access",
            OperationId = "DeleteProductReview",
            Tags = new[] { "Customer", "Customer_ProductReviews" }
        )]
        public async Task<IActionResult> DeleteProductReview(Guid id)
        {
            var command = new DeleteProductReviewCommand(id);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}