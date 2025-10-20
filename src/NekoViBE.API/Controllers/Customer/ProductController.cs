using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Product.Queries.GetProduct;
using NekoViBE.Application.Features.Product.Queries.GetProductList;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/products")]
    [ApiExplorerSettings(GroupName = "v1")]
    [SwaggerTag("This API is used for customer-facing product retrieval")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IMediator mediator, ILogger<ProductController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<ProductItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<ProductItem>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginationResult<ProductItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all products with pagination and filtering",
            Description = "This API retrieves a paginated list of products with filtering options for customers",
            OperationId = "GetCustomerProductList",
            Tags = new[] { "Customer", "Customer_Product" }
        )]
        public async Task<IActionResult> GetProductList([FromQuery] ProductFilter filter, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                var errorMessageString = errorMessages.Any() ? string.Join(", ", errorMessages) : "No error details available";
                _logger.LogWarning("Invalid model state for GetProductList: {Errors}", errorMessageString);
                return BadRequest(Result.Failure("Invalid request parameters", ErrorCodeEnum.InvalidInput));
            }

            _logger.LogInformation("Retrieving product list with filter: {@Filter}", filter);
            var query = new GetProductListQuery(filter);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve product list: {Errors}", string.Join(", ", result.Errors ?? Enumerable.Empty<string>()));
                return StatusCode(result.GetHttpStatusCode(), result);
            }

            _logger.LogInformation("Product list retrieved successfully, TotalItems: {TotalItems}", result.TotalItems);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<ProductResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<ProductResponse>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get product by ID",
            Description = "This API retrieves a specific product by its ID for customers",
            OperationId = "GetCustomerProduct",
            Tags = new[] { "Customer", "Customer_Product" }
        )]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var query = new GetProductQuery(id);
            var result = await _mediator.Send(query);
            return StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}