namespace NekoViBE.Application.Common.Models;

public class PaymentRequest
{
    public string? OrderId { get; set; } 
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string Description { get; set; } = string.Empty;
}