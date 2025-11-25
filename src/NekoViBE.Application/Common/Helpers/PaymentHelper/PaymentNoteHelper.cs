using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.Models.Momo;

namespace NekoViBE.Application.Common.Helpers.PaymentHelper;

/// <summary>
/// Helper class để generate note message cho Order và Payment dựa trên PaymentGatewayResult
/// </summary>
public static class PaymentNoteHelper
{
    /// <summary>
    /// Generate note message cho Order và Payment dựa trên PaymentGatewayResult
    /// </summary>
    /// <param name="paymentResult">Kết quả từ payment gateway</param>
    /// <param name="transactionResponse">Transaction response từ payment gateway (nếu có, ví dụ VnPayTransactionResponse hoặc MomoTransactionResponse)</param>
    /// <returns>Note message để lưu vào Order.Notes và Payment.Notes</returns>
    public static string GeneratePaymentNote(PaymentGatewayResult paymentResult, object? transactionResponse = null)
    {
        var noteParts = new List<string>
        {
            $"[Payment Gateway] {paymentResult.Message}"
        };

        if (transactionResponse != null)
        {
            // Handle VnPayTransactionResponse
            if (transactionResponse is VnPayTransactionResponse vnPayResponse)
            {
                noteParts.Add($"TransactionNo: {vnPayResponse.TransactionNo}");
                noteParts.Add($"ResponseCode: {vnPayResponse.ResponseCode}");
                noteParts.Add($"TransactionStatus: {vnPayResponse.TransactionStatus}");
                noteParts.Add($"Amount: {vnPayResponse.Amount:N0} VND");
                
                if (vnPayResponse.PaymentDate != default)
                {
                    noteParts.Add($"PaymentDate: {vnPayResponse.PaymentDate:yyyy-MM-dd HH:mm:ss} UTC");
                }
            }
            // Handle MomoTransactionResponse
            else if (transactionResponse is MomoTransactionResponse momoResponse)
            {
                noteParts.Add($"TransId: {momoResponse.TransId}");
                noteParts.Add($"ResultCode: {momoResponse.ResultCode}");
                noteParts.Add($"Message: {momoResponse.Message}");
                noteParts.Add($"Amount: {momoResponse.Amount:N0} VND");
                noteParts.Add($"OrderInfo: {momoResponse.OrderInfo}");
                
                if (momoResponse.ResponseTime != default)
                {
                    noteParts.Add($"ResponseTime: {momoResponse.ResponseTime:yyyy-MM-dd HH:mm:ss} UTC");
                }
            }
        }

        noteParts.Add($"ProcessedAt: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        noteParts.Add($"Status: {(paymentResult.IsSuccess ? "SUCCESS" : "FAILED")}");

        return string.Join(" | ", noteParts);
    }
}

