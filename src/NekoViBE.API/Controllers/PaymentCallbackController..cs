using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Features.Payment.Commands;
using PaymentService.Application.Commons.Models.Momo;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers;

[ApiController]
[Route("api/payment-callback")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("PaymentGateway", "PaymentGateway_Ipn")]
[SwaggerTag("This API is used for payment IPN processing")]
public class PaymentCallbackController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentCallbackController> _logger;

    public PaymentCallbackController(IMediator mediator, ILogger<PaymentCallbackController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("vnpay/IPN")]
    [SwaggerOperation(
        Summary = "Process VNPay IPN callback",
        Description = "This API processes the VNPay IPN callback request",
        OperationId = "ProcessVnPayIpnCallback",
        Tags = new[] { "PaymentGateway", "PaymentGateway_VnPayIpnCallback" }
    )]
    public async Task<IActionResult> ProcessVnPayIpn()
    {

        var result = await _mediator.Send(new ProcessVnPayCallbackCommand(Request.Query));
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        else
        {
            return Ok(new { RspCode = "02", Message = "Failed" });
        }
    }

    [HttpPost("momo/ipn")]
    [SwaggerOperation(
        Summary = "Process MoMo IPN callback",
        Description = "This API processes the MoMo IPN callback request. MoMo sends POST request with JSON body.",
        OperationId = "ProcessMomoIpnCallback",
        Tags = new[] { "PaymentGateway", "PaymentGateway_MomoIpnCallback" }
    )]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> ProcessMomoIpn([FromBody] MoMoIpnRequest request)
    {
        // if (request == null)
        // {
        //     _logger.LogWarning("[MoMo IPN] Received null request");
        //     return BadRequest(new { resultCode = 20, message = "Request body is required" });
        // }
        
        var callerIp = GetCallerIpAddress();
        
        _logger.LogInformation(
            "[MoMo IPN] Received callback - OrderId: {OrderId}, TransId: {TransId}, ResultCode: {ResultCode}, Amount: {Amount}, CallerIP: {CallerIP}",
            request.OrderId, request.TransId, request.ResultCode, request.Amount, callerIp);
        
        var result = await _mediator.Send(new ProcessMomoCallbackCommand(request, callerIp));
        
        _logger.LogInformation(
            "[MoMo IPN] Response sent - OrderId: {OrderId}, ResultCode: {ResultCode}, Message: {Message}",
            result.Data?.OrderId, result.Data?.ResultCode, result.Data?.Message);
        
        // MoMo yêu cầu HTTP 200 bất kể backend xử lý thành công hoặc thất bại
        // Handler đã build MoMoIpnResponse kèm signature cho mọi trường hợp
        return Ok(result.Data);
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
}
