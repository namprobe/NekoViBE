using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Product.Commands.CreateProduct;
using NekoViBE.Application.Features.Product.Commands.DeleteProduct;
using NekoViBE.Application.Features.Product.Commands.UpdateProduct;
using NekoViBE.Application.Features.Product.Queries.GetProduct;
using NekoViBE.Application.Features.Product.Queries.GetProductList;
using NekoViBE.Application.Features.Product.Queries.GetSelectList;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.API.Controllers.Cms;

[ApiController]
[Route("api/cms/products")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_Product")]
[SwaggerTag("This API is used for Product management in CMS")]
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
    [AuthorizeRoles]
    [ProducesResponseType(typeof(PaginationResult<ProductItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<ProductItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<ProductItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<ProductItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all products with pagination and filtering",
        Description = "This API retrieves a paginated list of products with filtering options",
        OperationId = "GetProductList",
        Tags = new[] { "CMS", "CMS_Product" }
    )]
    public async Task<IActionResult> GetProductList([FromQuery] ProductFilter filter, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for GetProductList: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(Result.Failure("Invalid request parameters", ErrorCodeEnum.InvalidInput));
        }

        _logger.LogInformation("Retrieving product list with filter: {@Filter}", filter);
        var query = new GetProductListQuery(filter);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to retrieve product list: {Errors}", string.Join(", ", result.Errors));
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        _logger.LogInformation("Product list retrieved successfully, TotalItems: {TotalItems}", result.TotalItems);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(Result<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProductResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ProductResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ProductResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<ProductResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
            Summary = "Get product by ID",
            Description = "This API retrieves a specific product by its ID",
            OperationId = "GetProduct",
            Tags = new[] { "CMS", "CMS_Product" }
        )]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var query = new GetProductQuery(id);
        var result = await _mediator.Send(query);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpPost]
    [AuthorizeRoles]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Create a new product",
        Description = "This API creates a new product with optional image upload. It requires Admin role access",
        OperationId = "CreateProduct",
        Tags = new[] { "CMS", "CMS_Product" }
    )]
    public async Task<IActionResult> CreateProduct([FromForm] ProductRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(Result.Failure("Invalid request parameters", ErrorCodeEnum.InvalidInput));
        }

        var command = new CreateProductCommand(request);
        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(result.GetHttpStatusCode(), result);
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
            Summary = "Update an existing product",
            Description = "This API updates an existing product. It requires Admin role access",
            OperationId = "UpdateProduct",
            Tags = new[] { "CMS", "CMS_Product" }
        )]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] UpdateProductDto request)
    {
        var command = new UpdateProductCommand(id, request);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpDelete("{id}")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
            Summary = "Delete a product",
            Description = "This API deletes a product. It requires Admin role access",
            OperationId = "DeleteProduct",
            Tags = new[] { "CMS", "CMS_Product" }
        )]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpGet("select-list")]
    [AuthorizeRoles]
    [ProducesResponseType(typeof(List<ProductSelectItem>), StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "Get product select list",
        Description = "Get a simplified list of products for dropdown selection",
        OperationId = "GetProductSelectList",
        Tags = new[] { "CMS", "CMS_Product" }
    )]
    public async Task<IActionResult> GetProductSelectList([FromQuery] string? search)
    {
        var query = new GetProductSelectListQuery { Search = search };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}