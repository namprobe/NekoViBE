using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.Models.Momo;
using NekoViBE.Domain.Enums;
using PaymentService.Application.Commons.Models.Momo;
namespace NekoViBE.Infrastructure.Services;

public class MomoService : IPaymentGatewayService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MomoService> _logger;
    private readonly string _partnerCode;
    private readonly string _partnerName;
    private readonly string _storeId;
    private readonly string _ipnUrl;
    private readonly string _redirectUrl;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _apiEndpoint;

    public MomoService(IHttpClientFactory httpClientFactory, ILogger<MomoService> logger, IOptions<MoMoSettings> moMoSettings)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _partnerCode = moMoSettings.Value.MomoPartnerCode;
        _partnerName = moMoSettings.Value.MomoPartnerName;
        _storeId = moMoSettings.Value.MomoStoreId;
        _ipnUrl = moMoSettings.Value.MomoIpnUrl;
        _redirectUrl = moMoSettings.Value.MomoRedirectUrl;
        _accessKey = moMoSettings.Value.MomoAccessToken;
        _secretKey = moMoSettings.Value.MomoSecretKey;
        _apiEndpoint = moMoSettings.Value.MomoApiEndpoint;
    }

    public async Task<object?> CreatePaymentIntentAsync(PaymentRequest request)
    {
        try
        {
            var requestType = "captureWallet"; // ✅ MoMo AIO v2: captureWallet | qrcode | payWithATM
            var lang = "vi";
            var requestId = $"REQ_{request.OrderId}";

            // ✅ MoMo ExtraData: Base64 encode JSON format
            var extraData = string.Empty;
            if (request.Metadata != null && request.Metadata.Any())
            {
                var jsonData = JsonSerializer.Serialize(request.Metadata);
                extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonData));
            }

            var amount = (long)request.Amount;
            var orderId = request.OrderId;
            var orderInfo = request.Description;
        

            // Build signature
            var parameters = new Dictionary<string, string>
            {
                { "accessKey", _accessKey },
                { "partnerCode", _partnerCode },
                { "requestType", requestType },
                { "ipnUrl", _ipnUrl },
                { "redirectUrl", _redirectUrl }, 
                { "orderId", orderId },
                { "amount", amount.ToString() },
                { "orderInfo", orderInfo },
                { "requestId", requestId },
                { "extraData", extraData }
            };
            
            var signature = BuildSignature(parameters);
            
            var momoRequest = new CreateMomoRequest
            {
                PartnerCode = _partnerCode,
                PartnerName = _partnerName,
                StoreId = _storeId,
                RequestType = requestType,
                IpnUrl = _ipnUrl,
                RedirectUrl = _redirectUrl, 
                OrderId = orderId,
                Amount = amount,
                Lang = lang,
                OrderInfo = orderInfo,
                RequestId = requestId,
                ExtraData = extraData,
                Signature = signature
            };
            
            // ✅ Call MoMo API
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync($"{_apiEndpoint}/create", momoRequest);

            // ✅ Validate HTTP response
            response.EnsureSuccessStatusCode();

            // ✅ Parse and validate MoMo response
            var result = await response.Content.ReadFromJsonAsync<MomoResponse>(options: new JsonSerializerOptions { PropertyNamingPolicy  = JsonNamingPolicy.CamelCase })
                ?? throw new Exception("Failed to parse MoMo response");

            // ✅ Log response details for debugging
            _logger.LogInformation("MoMo Response - PayUrl: {PayUrl}, DeepLink: {DeepLink}, DeepLinkWebInApp: {DeepLinkWebInApp}", 
                result.PayUrl, result.DeepLink, result.DeepLinkWebInApp);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent");
            throw;
        }

    }

    /// <summary>
    /// Build HMAC SHA256 signature for MoMo request
    /// </summary>
    public string BuildSignature(Dictionary<string, string> parameters)
    {
        // Sort parameters alphabetically
        var sortedParams = parameters.OrderBy(x => x.Key);

        // Build raw signature string
        var rawSignature = string.Join("&",
            sortedParams.Select(p => $"{p.Key}={p.Value}"));

        // Calculate HMAC SHA256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawSignature));

        return BitConverter.ToString(hashBytes)
            .Replace("-", "")
            .ToLower();
    }

    public string GetProviderName()
    {
        return PaymentGatewayType.Momo.ToString();
    }

    public string BuildIpnResponseSignature(object response)
    {
        if (response is not MoMoIpnResponse momoResponse)
        {
            throw new ArgumentException("Response must be of type MoMoIpnResponse", nameof(response));
        }
        
        return BuildIpnResponseSignature(momoResponse);
    }

    /// <summary>
    /// Build HMAC SHA256 signature for MoMo IPN response
    /// Signature format: HMAC_SHA256(accessKey=$accessKey&extraData=$extraData&message=$message&orderId=$orderId&partnerCode=$partnerCode&requestId=$requestId&responseTime=$responseTime&resultCode=$resultCode,secretKey)
    /// </summary>
    public string BuildIpnResponseSignature(MoMoIpnResponse response)
    {
        var parameters = new Dictionary<string, string>
        {
            { "accessKey", _accessKey },
            { "extraData", response.ExtraData ?? string.Empty },
            { "message", response.Message ?? string.Empty },
            { "orderId", response.OrderId ?? string.Empty },
            { "partnerCode", response.PartnerCode ?? string.Empty },
            { "requestId", response.RequestId ?? string.Empty },
            { "responseTime", response.ResponseTime.ToString() },
            { "resultCode", response.ResultCode.ToString() }
        };

        return BuildSignature(parameters);
    }

    public Task<object?> RefundPaymentAsync(string transactionId, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public PaymentGatewayResult VerifyIpnRequest(object ipnRequest)
    {
        try
        {
            // Cast ipnRequest to MoMoIpnRequest
            var moMoIpnRequest = ipnRequest as MoMoIpnRequest ?? throw new ArgumentException("Invalid IPN request type", nameof(ipnRequest));
            
            // Build signature
            var parameters = new Dictionary<string, string>
            {
                { "accessKey", _accessKey },
                { "amount", moMoIpnRequest.Amount.ToString() },
                { "extraData", moMoIpnRequest.ExtraData },
                { "message", moMoIpnRequest.Message },
                { "orderId", moMoIpnRequest.OrderId },
                { "orderInfo", moMoIpnRequest.OrderInfo },
                { "orderType", moMoIpnRequest.OrderType },
                { "partnerCode", moMoIpnRequest.PartnerCode },
                { "payType", moMoIpnRequest.PayType },
                { "requestId", moMoIpnRequest.RequestId },
                { "responseTime", moMoIpnRequest.ResponseTime.ToString() },
                { "resultCode", moMoIpnRequest.ResultCode.ToString() },
                { "transId", moMoIpnRequest.TransId.ToString() }
            };
            var signature = BuildSignature(parameters);

            // Validate signature
            var isValid = signature.Equals(moMoIpnRequest.Signature, StringComparison.OrdinalIgnoreCase);
            if (!isValid)
            {
                _logger.LogWarning("Invalid IPN signature: OrderId={OrderId}, Expected={Expected}, Received={Received}", 
                    moMoIpnRequest.OrderId, signature, moMoIpnRequest.Signature);
                return new PaymentGatewayResult
                {
                    IsSuccess = false,
                    Message = "Invalid signature",
                    Data = null
                };
            }

            var (isSuccess, statusMessage) = MapMomoResultCode(moMoIpnRequest.ResultCode);

            var transactionResponse = new MomoTransactionResponse
            {
                Amount = moMoIpnRequest.Amount,
                OrderId = moMoIpnRequest.OrderId,
                OrderInfo = moMoIpnRequest.OrderInfo,
                TransId = moMoIpnRequest.TransId.ToString(),
                ResultCode = moMoIpnRequest.ResultCode,
                Message = moMoIpnRequest.Message,
                ResponseTime = ConvertTimestampToUtc(moMoIpnRequest.ResponseTime)
            };

            return new PaymentGatewayResult
            {
                IsSuccess = isSuccess,
                Message = statusMessage,
                Data = transactionResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating IPN request");
            return new PaymentGatewayResult
            {
                IsSuccess = false,
                Message = $"Lỗi xử lý IPN: {ex.Message}",
                Data = null
            };
        }
    }

    public async Task<object?> QueryTransactionAsync(object request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Cast request to MoMoQueryTransactionRequest
            var moMoQueryTransactionRequest = request as MoMoQueryTransactionRequest ?? throw new ArgumentException("Invalid request type", nameof(request));
            
            // Build signature
            var parameters = new Dictionary<string, string>
            {
                { "accessKey", _accessKey },
                { "orderId", moMoQueryTransactionRequest.OrderId },
                { "partnerCode", moMoQueryTransactionRequest.PartnerCode },
                { "requestId", moMoQueryTransactionRequest.RequestId },
            };
            var signature = BuildSignature(parameters);
            
            // Call query transaction API
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync($"{_apiEndpoint}/query", moMoQueryTransactionRequest, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<MoMoTransactionResultResponse>(
                options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }, 
                cancellationToken)
                ?? throw new InvalidOperationException("Failed to parse MoMo transaction result response");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying transaction");
            throw;
        }
    }

    private DateTime ConvertTimestampToUtc(long timestamp)
    {
        if (timestamp <= 0)
        {
            return DateTime.UtcNow;
        }
        try
        {
            // MoMo response time is timestamp (ms). Fall back to seconds if smaller.
            if (timestamp > 9999999999)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
            }
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        }
        catch
        {
            return DateTime.UtcNow;
        }
    }

    private (bool IsSuccess, string Message) MapMomoResultCode(int resultCode)
    {
        return resultCode switch
        {
            0 => (true, "Giao dịch thành công"),
            9000 => (true, "Giao dịch đã được xác nhận thành công"),
            8000 => (false, "Giao dịch đang chờ người dùng xác nhận lại"),
            7000 => (false, "Giao dịch đang được xử lý"),
            1000 => (false, "Giao dịch đang chờ người dùng xác nhận thanh toán"),
            1001 => (false, "Giao dịch thất bại do tài khoản người dùng không đủ tiền"),
            1002 => (false, "Giao dịch bị từ chối do nhà phát hành tài khoản"),
            1003 => (false, "Giao dịch đã bị hủy"),
            1004 => (false, "Giao dịch thất bại do vượt hạn mức thanh toán"),
            1005 => (false, "Giao dịch thất bại do url hoặc QR code đã hết hạn"),
            1006 => (false, "Giao dịch thất bại do người dùng từ chối thanh toán"),
            1007 => (false, "Giao dịch bị từ chối vì tài khoản người dùng đang tạm khóa"),
            1026 => (false, "Giao dịch bị hạn chế theo thể lệ chương trình khuyến mãi"),
            1080 => (false, "Hoàn tiền bị từ chối. Giao dịch thanh toán ban đầu không được tìm thấy"),
            1081 => (false, "Hoàn tiền bị từ chối. Giao dịch thanh toán ban đầu có thể đã được hoàn"),
            2001 => (false, "Giao dịch thất bại do sai thông tin liên kết"),
            2007 => (false, "Giao dịch thất bại do liên kết đang bị tạm khóa"),
            3001 => (false, "Liên kết thất bại do người dùng từ chối xác nhận"),
            3002 => (false, "Liên kết bị từ chối do không thỏa quy tắc liên kết"),
            3003 => (false, "Hủy liên kết bị từ chối do đã vượt quá số lần hủy"),
            3004 => (false, "Liên kết này không thể hủy do có giao dịch đang chờ xử lý"),
            4001 => (false, "Giao dịch bị hạn chế do người dùng chưa hoàn tất xác thực tài khoản"),
            4010 => (false, "Xác minh OTP thất bại"),
            4011 => (false, "OTP chưa được gửi hoặc đã hết hạn"),
            4015 => (false, "Xác minh 3DS thất bại"),
            4100 => (false, "Giao dịch thất bại do người dùng không đăng nhập thành công"),
            10 => (false, "Hệ thống đang được bảo trì"),
            11 => (false, "Truy cập bị từ chối"),
            12 => (false, "Phiên bản API không được hỗ trợ"),
            13 => (false, "Xác thực doanh nghiệp thất bại"),
            20 => (false, "Yêu cầu sai định dạng"),
            21 => (false, "Số tiền giao dịch không hợp lệ"),
            40 => (false, "RequestId bị trùng"),
            41 => (false, "OrderId bị trùng"),
            42 => (false, "OrderId không hợp lệ hoặc không được tìm thấy"),
            43 => (false, "Yêu cầu bị từ chối vì xung đột trong quá trình xử lý"),
            99 => (false, "Lỗi không xác định"),
            _ => (false, $"Giao dịch không thành công. Mã lỗi: {resultCode}")
        };
    }
}