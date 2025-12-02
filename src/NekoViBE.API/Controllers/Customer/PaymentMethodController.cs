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

namespace NekoViBE.API.Controllers.Customer;

/// <summary>
/// Controller quản lý Payment Methods cho CMS
/// </summary>
[ApiController]
[Route("api/customer/payment-methods")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Customer", "Customer_PaymentMethods")]
[SwaggerTag("This API is used for Payment Methods in Customer")]
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
    /// 
    /// Sample request:
    /// 
    ///     GET /api/customer/payment-methods?page=1&amp;pageSize=10&amp;search=VnPay&amp;isOnlinePayment=true&amp;sortBy=name&amp;isAscending=true
    /// 
    /// </remarks>
    /// <param name="filter">Payment method filter parameters</param>
    /// <returns>Paginated list of payment methods</returns>
    /// <response code="200">Payment methods retrieved successfully</response>
    /// <response code="500">Failed to retrieve payment methods (internal server error)</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResult<PaymentMethodItem>), StatusCodes.Status200OK)]
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
    /// 
    /// Sample request:
    /// 
    ///     GET /api/customer/payment-methods/123e4567-e89b-12d3-a456-426614174000
    /// 
    /// </remarks>
    /// <param name="id">Payment method ID</param>
    /// <returns>Payment method details</returns>
    /// <response code="200">Payment method retrieved successfully</response>
    /// <response code="404">Payment method not found</response>
    /// <response code="500">Failed to retrieve payment method (internal server error)</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<PaymentMethodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaymentMethodResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<PaymentMethodResponse>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get payment method by ID",
        Description = "This API retrieves a specific payment method by its ID",
        OperationId = "GetPaymentMethod",
        Tags = new[] { "Customer", "Customer_PaymentMethods" }
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
}
