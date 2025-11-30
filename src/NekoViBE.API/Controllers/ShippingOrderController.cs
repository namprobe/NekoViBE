using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.Models.GHN;
using NekoViBE.Application.Features.Shipping.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers;

[ApiController]
[Route("api/shipping-order")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Shipping", "Shipping_GHN")]
[SwaggerTag("This API is used for shipping order management with GHN")]
public class ShippingOrderController : ControllerBase
{
    private readonly IShippingService _shippingService;
    private readonly IMediator _mediator;
    private readonly ILogger<ShippingOrderController> _logger;

    public ShippingOrderController(
        IShippingService shippingService,
        IMediator mediator,
        ILogger<ShippingOrderController> logger)
    {
        _shippingService = shippingService;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("calculate-fee")]
    [SwaggerOperation(
        Summary = "Calculate shipping fee",
        Description = "Calculate shipping fee for an order based on weight, dimensions, and destination",
        OperationId = "CalculateShippingFee",
        Tags = new[] { "Shipping", "Shipping_GHN" }
    )]
    [ProducesResponseType(typeof(ShippingFeeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ShippingFeeResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateFee([FromBody] ShippingFeeRequest request)
    {
        var result = await _shippingService.CalculateFeeAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("preview")]
    [SwaggerOperation(
        Summary = "Preview order before creating",
        Description = "Preview shipping order information including fee and expected delivery time",
        OperationId = "PreviewShippingOrder",
        Tags = new[] { "Shipping", "Shipping_GHN" }
    )]
    [ProducesResponseType(typeof(ShippingPreviewResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ShippingPreviewResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreviewOrder([FromBody] ShippingOrderRequest request)
    {
        var result = await _shippingService.PreviewOrderAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("leadtime")]
    [SwaggerOperation(
        Summary = "Get expected delivery time",
        Description = "Get expected delivery time (lead time) for shipping",
        OperationId = "GetShippingLeadTime",
        Tags = new[] { "Shipping", "Shipping_GHN" }
    )]
    [ProducesResponseType(typeof(ShippingLeadTimeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ShippingLeadTimeResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeadTime([FromBody] ShippingLeadTimeRequest request)
    {
        var result = await _shippingService.GetLeadTimeAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("create")]
    [SwaggerOperation(
        Summary = "Create shipping order",
        Description = "Create a new shipping order with GHN",
        OperationId = "CreateShippingOrder",
        Tags = new[] { "Shipping", "Shipping_GHN" }
    )]
    [ProducesResponseType(typeof(ShippingOrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ShippingOrderResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] ShippingOrderRequest request)
    {
        _logger.LogInformation("Creating shipping order for client order code: {ClientOrderCode}", request.ClientOrderCode);
        
        var result = await _shippingService.CreateOrderAsync(request);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "Shipping order created successfully - OrderCode: {OrderCode}, ClientOrderCode: {ClientOrderCode}",
                result.Data?.OrderCode, request.ClientOrderCode);
        }
        else
        {
            _logger.LogWarning(
                "Failed to create shipping order - ClientOrderCode: {ClientOrderCode}, Message: {Message}",
                request.ClientOrderCode, result.Message);
        }
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("ghn/callback")]
    [SwaggerOperation(
        Summary = "GHN order status callback",
        Description = "This API receives callback from GHN when order status changes and updates order status in database",
        OperationId = "ProcessGHNCallback",
        Tags = new[] { "Shipping", "Shipping_GHN_Callback" }
    )]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessGHNCallback([FromBody] GHNCallbackRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("[GHN Callback] Received null request");
            return BadRequest(new { message = "Request body is required" });
        }

        var callerIp = GetCallerIpAddress();

        _logger.LogInformation(
            "[GHN Callback] Received callback - OrderCode: {OrderCode}, ClientOrderCode: {ClientOrderCode}, Status: {Status}, CallerIP: {CallerIP}",
            request.OrderCode, request.ClientOrderCode, request.Status, callerIp);

        var result = await _mediator.Send(new ProcessGHNCallbackCommand(request, callerIp));

        _logger.LogInformation(
            "[GHN Callback] Processed - OrderCode: {OrderCode}, Success: {Success}",
            request.OrderCode, result.IsSuccess);

        // GHN expects HTTP 200 response
        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return Ok(new { code = 200, message = result.Message ?? "Callback processed" });
    }

    private string GetCallerIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ips = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        if (Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            return realIp.ToString().Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    [HttpPost("ghn/simulate-callback")]
    [SwaggerOperation(
        Summary = "Simulate GHN webhook callback (for testing)",
        Description = "This API simulates a GHN webhook callback to test order status updates. Use this for development and testing purposes.",
        OperationId = "SimulateGHNCallback",
        Tags = new[] { "Shipping", "Shipping_GHN_Callback", "Testing" }
    )]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SimulateGHNCallback([FromBody] GHNCallbackRequest request)
    {
        if (request == null)
        {
            _logger.LogWarning("[GHN Simulate] Received null request");
            return BadRequest(new { message = "Request body is required" });
        }

        var callerIp = GetCallerIpAddress();

        _logger.LogInformation(
            "[GHN Simulate] Simulating callback - OrderCode: {OrderCode}, ClientOrderCode: {ClientOrderCode}, Status: {Status}, CallerIP: {CallerIP}",
            request.OrderCode, request.ClientOrderCode, request.Status, callerIp);

        // Use the same command handler as the real callback
        var result = await _mediator.Send(new ProcessGHNCallbackCommand(request, callerIp));

        _logger.LogInformation(
            "[GHN Simulate] Processed - OrderCode: {OrderCode}, Success: {Success}",
            request.OrderCode, result.IsSuccess);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(result.Data);
        }

        return Ok(new { code = 200, message = result.Message ?? "Simulation processed" });
    }
}

