using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using NekoViBE.Infrastructure.Utils;

namespace NekoViBE.Infrastructure.Services;

public class VnPayService : IPaymentGatewayService
{
    private readonly VnPaySettings _vnPaySettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly VnPayLibrary _vnPayLibrary;
    
    public VnPayService(IOptions<VnPaySettings> vnPaySettings, IHttpContextAccessor httpContextAccessor)
    {
        _vnPaySettings = vnPaySettings.Value;
        _httpContextAccessor = httpContextAccessor;
        _vnPayLibrary = new VnPayLibrary(httpContextAccessor);
    }

    public Task<object?> CreatePaymentIntentAsync(PaymentRequest request)
    {
        string baseUrl = _vnPaySettings.BaseUrl ?? throw new InvalidOperationException("VNPay:BaseUrl not configured");
        string vnp_HashSecret = _vnPaySettings.HashSecret ?? throw new InvalidOperationException("VNPay:HashSecret not configured");
        string vnp_ReturnUrl = _vnPaySettings.ReturnUrl ?? throw new InvalidOperationException("VNPay:ReturnUrl not configured");
        string vnp_TmnCode = _vnPaySettings.TmnCode ?? throw new InvalidOperationException("VNPay:TmnCode not configured");
        
        // Clear previous data
        _vnPayLibrary.ClearRequestData();
        
        // Convert UTC to Vietnam timezone (GMT+7) for VNPay
        var vietnamCreateDate = VnPayDateTimeHelper.GetCurrentVietnamTimeString();
        var vietnamExpireDate = VnPayDateTimeHelper.GetExpireTimeString(15); // Default 15 minutes expiry
        
        // Add required parameters
        _vnPayLibrary.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        _vnPayLibrary.AddRequestData("vnp_Command", "pay");
        _vnPayLibrary.AddRequestData("vnp_TmnCode", vnp_TmnCode);
        _vnPayLibrary.AddRequestData("vnp_Amount", (request.Amount * 100).ToString()); // Convert to smallest currency unit
        _vnPayLibrary.AddRequestData("vnp_CurrCode", request.Currency);
        _vnPayLibrary.AddRequestData("vnp_BankCode", ""); // Optional: empty for user to choose payment method
        _vnPayLibrary.AddRequestData("vnp_CreateDate", vietnamCreateDate);
        _vnPayLibrary.AddRequestData("vnp_ExpireDate", vietnamExpireDate);
        _vnPayLibrary.AddRequestData("vnp_IpAddr", _vnPayLibrary.GetClientIpAddress());
        _vnPayLibrary.AddRequestData("vnp_Locale", "vn");
        // VNPay yêu cầu vnp_OrderInfo: Tiếng Việt không dấu và không bao gồm các ký tự đặc biệt
        // Loại bỏ dấu tiếng Việt và chuyển về chữ thường
        string orderInfo = RemoveVietnameseAccents(request.Description ?? "Thanh toan don hang");
        _vnPayLibrary.AddRequestData("vnp_OrderInfo", orderInfo);
        _vnPayLibrary.AddRequestData("vnp_OrderType", _vnPaySettings.OrderType);
        _vnPayLibrary.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
        _vnPayLibrary.AddRequestData("vnp_TxnRef", request.OrderId);
        
        // Create payment URL
        return Task.FromResult<object?>(_vnPayLibrary.CreateRequestUrl(baseUrl, vnp_HashSecret));
    }
    
    public (bool IsSuccess, string Message, string TransactionId) GetPaymentResult(IQueryCollection queryParams)
    {
        try
        {
            var responseCode = queryParams["vnp_ResponseCode"].ToString();
            var transactionStatus = queryParams["vnp_TransactionStatus"].ToString();
            var transactionId = queryParams["vnp_TransactionNo"].ToString();
            var orderInfo = queryParams["vnp_OrderInfo"].ToString();
            
            // Check if payment was successful
            bool isSuccess = responseCode == "00" && transactionStatus == "00";
            
            string message = isSuccess 
                ? "Thanh toán thành công" 
                : $"Thanh toán thất bại. Mã lỗi: {responseCode}";
            
            return (isSuccess, message, transactionId);
        }
        catch
        {
            return (false, "Không thể xử lý kết quả thanh toán", string.Empty);
        }
    }

    public string GetProviderName()
    {
        return PaymentGatewayType.VnPay.ToString();
    }

    public string BuildIpnResponseSignature(object response)
    {
        // VNPay không yêu cầu signed response cho IPN callback
        throw new NotImplementedException("VNPay does not require signed IPN responses");
    }

    public PaymentGatewayResult VerifyIpnRequest(object ipnRequest)
    {
        try
        {
            var queryParams = ipnRequest as IQueryCollection;
            if (queryParams == null)
            {
                return new PaymentGatewayResult
                {
                    IsSuccess = false,
                    Message = "Ipn request is not a query collection",
                    Data = null
                };
            }
            string vnp_HashSecret = _vnPaySettings.HashSecret ?? throw new InvalidOperationException("VNPay:HashSecret not configured");
            
            // Clear previous response data
            _vnPayLibrary.ClearResponseData();
            
            // Add all response parameters
            foreach (var param in queryParams)
            {
                _vnPayLibrary.AddResponseData(param.Key, param.Value.ToString());
            }
            
            // Get the secure hash from response
            string inputHash = _vnPayLibrary.GetResponseData("vnp_SecureHash");
            
            // Validate signature
            bool isValid = _vnPayLibrary.ValidateSignature(inputHash, vnp_HashSecret);
            if (!isValid)
            {
                return new PaymentGatewayResult
                {
                    IsSuccess = false,
                    Message = "Invalid signature",
                    Data = null
                };
            }
            // Lấy thông tin transaction
            var responseCode = queryParams["vnp_ResponseCode"].ToString() ?? string.Empty;
            var transactionStatus = queryParams["vnp_TransactionStatus"].ToString() ?? string.Empty;
            
            // Parse payment date từ VNPay (GMT+7) và chuyển về UTC
            DateTime paymentDateUtc = DateTime.UtcNow;
            var payDateString = queryParams["vnp_PayDate"].ToString();
            if (!string.IsNullOrWhiteSpace(payDateString))
            {
                try
                {
                    paymentDateUtc = VnPayDateTimeHelper.ParseVietnamTimeStringToUtc(payDateString);
                }
                catch
                {
                    // Nếu parse lỗi, sử dụng UTC now
                    paymentDateUtc = DateTime.UtcNow;
                }
            }
            
            // Tạo VnPayTransactionResponse
            var vnPayTransactionResponse = new VnPayTransactionResponse
            {
                TnxRef = queryParams["vnp_TxnRef"].ToString() ?? string.Empty,
                TransactionNo = queryParams["vnp_TransactionNo"].ToString() ?? string.Empty,
                ResponseCode = responseCode,
                TransactionStatus = transactionStatus,
                Amount = decimal.Parse(queryParams["vnp_Amount"].ToString() ?? "0") / 100, // Convert back from smallest currency unit
                PaymentDate = paymentDateUtc
            };

            // Xác định kết quả thanh toán dựa trên response code và transaction status
            var paymentResult = DeterminePaymentResult(responseCode, transactionStatus);
            
            return new PaymentGatewayResult
            {
                IsSuccess = paymentResult.IsSuccess,
                Message = paymentResult.Message,
                Data = vnPayTransactionResponse
            };
        }
        catch (Exception ex)
        {
            return new PaymentGatewayResult
            {
                IsSuccess = false,
                Message = $"Lỗi xử lý IPN: {ex.Message}",
                Data = null
            };
        }
    }

    /// <summary>
    /// Helper method để xác định kết quả thanh toán dựa trên response code và transaction status
    /// </summary>
    private (bool IsSuccess, string Message) DeterminePaymentResult(string responseCode, string transactionStatus)
    {
        // Kiểm tra transaction status trước
        if (transactionStatus != "00")
        {
            var statusMessage = GetTransactionStatusMessage(transactionStatus);
            return (false, statusMessage);
        }

        // Kiểm tra response code
        return GetResponseCodeResult(responseCode);
    }

    /// <summary>
    /// Lấy message tương ứng với transaction status
    /// </summary>
    private string GetTransactionStatusMessage(string transactionStatus)
    {
        return transactionStatus switch
        {
            "00" => "Giao dịch thành công",
            "01" => "Giao dịch chưa hoàn tất",
            "02" => "Giao dịch bị lỗi",
            "04" => "Giao dịch đảo (Khách hàng đã bị trừ tiền tại Ngân hàng nhưng GD chưa thành công ở VNPAY)",
            "05" => "VNPAY đang xử lý giao dịch này (GD hoàn tiền)",
            "06" => "VNPAY đã gửi yêu cầu hoàn tiền sang Ngân hàng (GD hoàn tiền)",
            "07" => "Giao dịch bị nghi ngờ gian lận",
            "09" => "GD Hoàn trả bị từ chối",
            _ => $"Giao dịch không thành công. Mã trạng thái: {transactionStatus}"
        };
    }

    /// <summary>
    /// Lấy kết quả và message tương ứng với response code
    /// </summary>
    private (bool IsSuccess, string Message) GetResponseCodeResult(string responseCode)
    {
        return responseCode switch
        {
            "00" => (true, "Giao dịch thành công"),
            "07" => (false, "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)"),
            "09" => (false, "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng"),
            "10" => (false, "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần"),
            "11" => (false, "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch"),
            "12" => (false, "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa"),
            "13" => (false, "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP). Xin quý khách vui lòng thực hiện lại giao dịch"),
            "24" => (false, "Giao dịch không thành công do: Khách hàng hủy giao dịch"),
            "51" => (false, "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch"),
            "65" => (false, "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày"),
            "75" => (false, "Ngân hàng thanh toán đang bảo trì"),
            "79" => (false, "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định. Xin quý khách vui lòng thực hiện lại giao dịch"),
            "99" => (false, "Các lỗi khác (lỗi còn lại, không có trong danh sách mã lỗi đã liệt kê)"),
            _ => (false, $"Giao dịch không thành công. Mã lỗi: {responseCode}")
        };
    }

    /// <summary>
    /// Loại bỏ dấu tiếng Việt và chuyển về chữ thường cho vnp_OrderInfo
    /// </summary>
    private string RemoveVietnameseAccents(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Normalize và loại bỏ dấu
        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder result = new StringBuilder();
        
        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            // Chỉ giữ lại chữ cái, số, khoảng trắng và một số ký tự cơ bản
            if (category == UnicodeCategory.LowercaseLetter ||
                category == UnicodeCategory.UppercaseLetter ||
                category == UnicodeCategory.DecimalDigitNumber ||
                category == UnicodeCategory.SpaceSeparator ||
                c == '.' || c == ',' || c == '-' || c == '_' || c == ':')
            {
                result.Append(c);
            }
        }
        
        // Chuyển thành chữ thường và loại bỏ khoảng trắng thừa
        return result.ToString().ToLower().Trim();
    }
}