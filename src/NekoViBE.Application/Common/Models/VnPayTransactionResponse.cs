namespace NekoViBE.Application.Common.Models;

public class VnPayTransactionResponse
{
    public string TnxRef { get; set; } = string.Empty;
    public string TransactionNo { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 0;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}