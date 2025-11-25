namespace NekoViBE.Application.Common.Models;

public class PaymentGatewayResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}