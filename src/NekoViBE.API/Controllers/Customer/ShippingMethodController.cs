using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Application.Common.Models;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Features.ShippingMethod.Queries.GetShippingMethods;
using NekoViBE.Application.Features.ShippingMethod.Queries.GetShippingMethod;
using NekoViBE.Application.Features.ShippingMethod.Queries.CalculateShippingFee;
using NekoViBE.Application.Features.ShippingMethod.Queries.CalculateShippingLeadTime;
using NekoViBE.Application.Common.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace NekoViBE.API.Controllers.Customer;

/// <summary>
/// Controller quản lý Shipping Methods cho Customer
/// </summary>
[ApiController]
[Route("api/customer/shipping-methods")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Customer", "Customer_ShippingMethods")]
[SwaggerTag("This API is used for Shipping Methods in Customer")]
public class ShippingMethodController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingMethodController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all shipping methods with pagination and filtering
    /// </summary>
    /// <remarks>
    /// This API retrieves a paginated list of shipping methods with filtering options.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/customer/shipping-methods?page=1&amp;pageSize=10&amp;search=GHN&amp;sortBy=cost&amp;isAscending=true
    /// 
    /// </remarks>
    /// <param name="filter">Shipping method filter parameters</param>
    /// <returns>Paginated list of shipping methods</returns>
    /// <response code="200">Shipping methods retrieved successfully</response>
    /// <response code="500">Failed to retrieve shipping methods (internal server error)</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResult<ShippingMethodItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<ShippingMethodItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all shipping methods with pagination and filtering",
        Description = "This API retrieves a paginated list of shipping methods with filtering options",
        OperationId = "GetShippingMethods",
        Tags = new[] { "Customer", "Customer_ShippingMethods" }
    )]
    public async Task<IActionResult> GetShippingMethods([FromQuery] ShippingMethodFilter filter)
    {
        var query = new GetShippingMethodsQuery(filter);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get shipping method by ID
    /// </summary>
    /// <remarks>
    /// This API retrieves a specific shipping method by its ID.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/customer/shipping-methods/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// </remarks>
    /// <param name="id">Shipping method ID</param>
    /// <returns>Shipping method details</returns>
    /// <response code="200">Shipping method retrieved successfully</response>
    /// <response code="404">Shipping method not found</response>
    /// <response code="500">Failed to retrieve shipping method (internal server error)</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<ShippingMethodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ShippingMethodResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<ShippingMethodResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get shipping method by ID",
        Description = "This API retrieves a specific shipping method by its ID",
        OperationId = "GetShippingMethod",
        Tags = new[] { "Customer", "Customer_ShippingMethods" }
    )]
    public async Task<IActionResult> GetShippingMethod(Guid id)
    {
        var query = new GetShippingMethodQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Calculate shipping fee for an order
    /// </summary>
    /// <remarks>
    /// This API calculates shipping fee based on shipping method, user address, and products/cart items.
    /// 
    /// Two cases:
    /// 1. Buy now: Provide ProductId and Quantity
    /// 2. Buy from cart: Do not provide ProductId (items are taken from the authenticated user's cart)
    /// 
    /// Sample request (Buy now):
    /// 
    ///     POST /api/customer/shipping-methods/calculate-fee
    ///     {
    ///         "shippingMethodId": "123e4567-e89b-12d3-a456-426614174000",
    ///         "userAddressId": "123e4567-e89b-12d3-a456-426614174001",
    ///         "productId": "123e4567-e89b-12d3-a456-426614174002",
    ///         "quantity": 2,
    ///         "insuranceValue": 500000,
    ///         "codValue": 500000
    ///     }
    /// 
    /// Sample request (Buy from cart):
    /// 
    ///     POST /api/customer/shipping-methods/calculate-fee
    ///     {
    ///         "shippingMethodId": "123e4567-e89b-12d3-a456-426614174000",
    ///         "userAddressId": "123e4567-e89b-12d3-a456-426614174001",
    ///         "insuranceValue": 1000000,
    ///         "codValue": 1000000
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">Calculate shipping fee request</param>
    /// <returns>Shipping fee result</returns>
    /// <response code="200">Shipping fee calculated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Shipping method or address not found</response>
    /// <response code="500">Failed to calculate shipping fee (internal server error)</response>
    [HttpPost("calculate-fee")]
    [Authorize]
    [ProducesResponseType(typeof(Result<ShippingFeeResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ShippingFeeResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ShippingFeeResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<ShippingFeeResult>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Calculate shipping fee",
        Description = "Calculate shipping fee for an order based on shipping method, user address, and products/cart items",
        OperationId = "CalculateShippingFee",
        Tags = new[] { "Customer", "Customer_ShippingMethods" }
    )]
    public async Task<IActionResult> CalculateShippingFee([FromBody] CalculateShippingFeeRequest request)
    {
        var query = new CalculateShippingFeeQuery(request);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get estimated delivery lead time
    /// </summary>
    /// <remarks>
    /// Similar to calculate shipping fee, this API returns the expected delivery timestamps for the provided shipping method and address.
    /// </remarks>
    [HttpPost("lead-time")]
    [Authorize]
    [ProducesResponseType(typeof(Result<ShippingLeadTimeResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ShippingLeadTimeResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ShippingLeadTimeResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<ShippingLeadTimeResult>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get estimated delivery lead time",
        Description = "Calculate the expected delivery lead time for an order based on shipping method and user address",
        OperationId = "CalculateShippingLeadTime",
        Tags = new[] { "Customer", "Customer_ShippingMethods" }
    )]
    public async Task<IActionResult> CalculateShippingLeadTime([FromBody] CalculateShippingFeeRequest request)
    {
        var query = new CalculateShippingLeadTimeQuery(request);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }
}

