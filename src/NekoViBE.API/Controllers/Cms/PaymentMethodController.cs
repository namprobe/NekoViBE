using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Application.Common.DTOs.PaymentMethod;
using NekoViBE.Application.Common.Models;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Features.PaymentMethod.Commands.CreatePaymentMethod;
using NekoViBE.Application.Features.PaymentMethod.Commands.UpdatePaymentMethod;
using NekoViBE.Application.Features.PaymentMethod.Commands.DeletePayment;
using NekoViBE.Application.Features.PaymentMethod.Queries.GetPaymentMethods;
using NekoViBE.Application.Features.PaymentMethod.Queries.GetPaymentMethod;
using Microsoft.AspNetCore.Authorization;
using NekoViBE.Application.Common.Extensions;

namespace NekoViBE.API.Controllers.Cms;

/// <summary>
/// Controller quản lý Payment Methods cho CMS
/// </summary>
[ApiController]
[Route("api/cms/payment-methods")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS", "CMS_PaymentMethods")]
[SwaggerTag("This API is used for Payment Methods management in CMS")]
public class PaymentMethodController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentMethodController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all payment methods with pagination and filtering
    /// </summary>
    /// <remarks>
    /// This API retrieves a paginated list of payment methods with filtering options.
    /// It requires Admin or Staff role access.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/payment-methods?page=1&amp;pageSize=10&amp;search=VnPay&amp;isOnlinePayment=true&amp;sortBy=name&amp;isAscending=true
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="filter">Payment method filter parameters</param>
    /// <returns>Paginated list of payment methods</returns>
    /// <response code="200">Payment methods retrieved successfully</response>
    /// <response code="401">Failed to retrieve payment methods (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="500">Failed to retrieve payment methods (internal server error)</response>
    [HttpGet]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(PaginationResult<PaymentMethodItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginationResult<PaymentMethodItem>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(PaginationResult<PaymentMethodItem>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(PaginationResult<PaymentMethodItem>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all payment methods with pagination and filtering",
        Description = "This API retrieves a paginated list of payment methods with filtering options",
        OperationId = "GetPaymentMethods",
        Tags = new[] { "CMS", "CMS_PaymentMethods" }
    )]
    public async Task<IActionResult> GetPaymentMethods([FromQuery] PaymentMethodFilter filter)
    {
        var query = new GetPaymentMethodsQuery(filter);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get payment method by ID
    /// </summary>
    /// <remarks>
    /// This API retrieves a specific payment method by its ID.
    /// It requires Admin or Staff role access.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/cms/payment-methods/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">Payment method ID</param>
    /// <returns>Payment method details</returns>
    /// <response code="200">Payment method retrieved successfully</response>
    /// <response code="401">Failed to retrieve payment method (not authorized)</response>
    /// <response code="403">No access (user is not a CMS member)</response>
    /// <response code="404">Payment method not found</response>
    /// <response code="500">Failed to retrieve payment method (internal server error)</response>
    [HttpGet("{id}")]
    [AuthorizeRoles("Admin", "Staff")]
    [ProducesResponseType(typeof(Result<PaymentMethodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaymentMethodResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<PaymentMethodResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<PaymentMethodResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<PaymentMethodResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get payment method by ID",
        Description = "This API retrieves a specific payment method by its ID",
        OperationId = "GetPaymentMethod",
        Tags = new[] { "CMS", "CMS_PaymentMethods" }
    )]
    public async Task<IActionResult> GetPaymentMethod(Guid id)
    {
        var query = new GetPaymentMethodQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new payment method
    /// </summary>
    /// <remarks>
    /// This API creates a new payment method. It requires Admin role access.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/cms/payment-methods
    ///     Content-Type: multipart/form-data
    ///     
    ///     {
    ///        "name": "VnPay",
    ///        "description": "Vietnam Payment Gateway",
    ///        "iconImage": [file],
    ///        "isOnlinePayment": true,
    ///        "processingFee": 2.5,
    ///        "processorName": "VnPay",
    ///        "configuration": "{\"merchantId\":\"123\",\"secretKey\":\"abc\"}",
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="request">Payment method creation request</param>
    /// <returns>Creation result</returns>
    /// <response code="200">Payment method created successfully</response>
    /// <response code="400">Creation failed (validation error)</response>
    /// <response code="401">Failed to create payment method (not authorized)</response>
    /// <response code="403">No access (user is not Admin)</response>
    /// <response code="500">Failed to create payment method (internal server error)</response>
    [HttpPost]
    [AuthorizeRoles("Admin")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Create a new payment method",
        Description = "This API creates a new payment method. It requires Admin role access",
        OperationId = "CreatePaymentMethod",
        Tags = new[] { "CMS", "CMS_PaymentMethods" }
    )]
    public async Task<IActionResult> CreatePaymentMethod([FromForm] PaymentMethodRequest request)
    {
        var command = new CreatePaymentMethodCommand(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Update an existing payment method
    /// </summary>
    /// <remarks>
    /// This API updates an existing payment method. It requires Admin role access.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/cms/payment-methods/123e4567-e89b-12d3-a456-426614174000
    ///     Content-Type: multipart/form-data
    ///     
    ///     {
    ///        "name": "VnPay Updated",
    ///        "description": "Updated Vietnam Payment Gateway",
    ///        "iconImage": [file],
    ///        "isOnlinePayment": true,
    ///        "processingFee": 3.0,
    ///        "processorName": "VnPay",
    ///        "configuration": "{\"merchantId\":\"456\",\"secretKey\":\"def\"}",
    ///        "status": 1
    ///     }
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">Payment method ID</param>
    /// <param name="request">Payment method update request</param>
    /// <returns>Update result</returns>
    /// <response code="200">Payment method updated successfully</response>
    /// <response code="400">Update failed (validation error)</response>
    /// <response code="401">Failed to update payment method (not authorized)</response>
    /// <response code="403">No access (user is not Admin)</response>
    /// <response code="404">Payment method not found</response>
    /// <response code="500">Failed to update payment method (internal server error)</response>
    [HttpPut("{id}")]
    [AuthorizeRoles("Admin")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Update an existing payment method",
        Description = "This API updates an existing payment method. It requires Admin role access",
        OperationId = "UpdatePaymentMethod",
        Tags = new[] { "CMS", "CMS_PaymentMethods" }
    )]
    public async Task<IActionResult> UpdatePaymentMethod(Guid id, [FromForm] PaymentMethodRequest request)
    {
        var command = new UpdatePaymentMethodCommand(id, request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a payment method
    /// </summary>
    /// <remarks>
    /// This API deletes a payment method. It requires Admin role access.
    /// The payment method cannot be deleted if it's currently in use by any payments.
    /// 
    /// Sample request:
    /// 
    ///     DELETE /api/cms/payment-methods/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// Headers:
    ///     Authorization: Bearer &lt;access_token&gt;
    /// </remarks>
    /// <param name="id">Payment method ID</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Payment method deleted successfully</response>
    /// <response code="401">Failed to delete payment method (not authorized)</response>
    /// <response code="403">No access (user is not Admin)</response>
    /// <response code="404">Payment method not found</response>
    /// <response code="409">Payment method is in use and cannot be deleted</response>
    /// <response code="500">Failed to delete payment method (internal server error)</response>
    [HttpDelete("{id}")]
    [AuthorizeRoles("Admin")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Delete a payment method",
        Description = "This API deletes a payment method. It requires Admin role access",
        OperationId = "DeletePaymentMethod",
        Tags = new[] { "CMS", "CMS_PaymentMethods" }
    )]
    public async Task<IActionResult> DeletePaymentMethod(Guid id)
    {
        var command = new DeletePaymentCommand(id);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return StatusCode(result.GetHttpStatusCode(), result);
        }
        return Ok(result);
    }
}
