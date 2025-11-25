namespace NekoViBE.Application.Common.Models.Momo;

public class MomoTransactionResponse
{
    public decimal Amount { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string OrderInfo { get; set; } = string.Empty;
    public string TransId { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ResponseTime {get;set;}
}