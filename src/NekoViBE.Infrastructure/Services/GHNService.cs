using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.Models.GHN;

namespace NekoViBE.Infrastructure.Services;

public class GHNService : IShippingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GHNService> _logger;
    private readonly GHNSettings _settings;

    // Shop default address (TP.HCM)
    private const string DefaultFromProvinceName = "Hồ Chí Minh";
    private const string DefaultFromDistrictName = "Quận 9";
    private const string DefaultFromWardName = "Long Thạnh Mỹ";
    private const string DefaultFromAddress = "Lô E2a-1, E2a-2, đường D1, Đ. D1, Long Thạnh Mỹ, Quận 9, Hồ Chí Minh, Vietnam";
    private const string DefaultFromName = "NekoVi Store";
    private const string DefaultFromPhone = "0867619150";
    // NOTE: DistrictId for Quận 9, Hồ Chí Minh - cần cập nhật từ GHN API khi có
    // Có thể lấy từ API: https://api.ghn.vn/home/docs/detail?id=90
    private const int DefaultFromDistrictId = 0; // TODO: Update with actual DistrictId from GHN API

    // Default package dimensions and weight (hard-coded since Product entity doesn't have these fields)
    private const int DefaultLength = 20; // cm
    private const int DefaultWidth = 20; // cm
    private const int DefaultHeight = 20; // cm
    private const int DefaultWeight = 500; // grams

    public GHNService(
        IHttpClientFactory httpClientFactory,
        ILogger<GHNService> logger,
        IOptions<GHNSettings> ghnSettings)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _settings = ghnSettings.Value;
    }

    public async Task<ShippingFeeResult> CalculateFeeAsync(ShippingFeeRequest request)
    {
        try
        {
            var ghnRequest = new GHNFeeRequest
            {
                FromDistrictId = request.FromDistrictId > 0 ? request.FromDistrictId : null,
                FromWardCode = null, // Optional, will be taken from ShopId if not provided
                ToDistrictId = request.ToDistrictId,
                ToWardCode = request.ToWardCode,
                ServiceTypeId = request.ServiceTypeId,
                Weight = request.Weight > 0 ? request.Weight : DefaultWeight,
                Length = request.Length > 0 ? request.Length : DefaultLength,
                Width = request.Width > 0 ? request.Width : DefaultWidth,
                Height = request.Height > 0 ? request.Height : DefaultHeight,
                InsuranceValue = request.InsuranceValue,
                CodValue = request.CodAmount, // Map CodAmount to CodValue for GHN API
                CodFailedAmount = null, // Optional
                Coupon = request.Coupon,
                Items = null // Only required for service_type_id = 5 (Hàng nặng), currently supporting service_type_id = 2 (Hàng nhẹ)
            };

            var httpClient = CreateHttpClient();
            var response = await httpClient.PostAsJsonAsync($"{_settings.BaseUrl}{_settings.GetFeePrefix}", ghnRequest);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GHNFeeResponse>(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (result == null)
            {
                return new ShippingFeeResult
                {
                    IsSuccess = false,
                    Message = "Failed to parse GHN response"
                };
            }

            if (result.Code != 200)
            {
                return new ShippingFeeResult
                {
                    IsSuccess = false,
                    Message = result.Message
                };
            }

            return new ShippingFeeResult
            {
                IsSuccess = true,
                Message = result.Message,
                Data = result.Data != null ? new ShippingFeeData
                {
                    Total = result.Data.Total,
                    ServiceFee = result.Data.ServiceFee,
                    InsuranceFee = result.Data.InsuranceFee,
                    PickStationFee = result.Data.PickStationFee,
                    CouponValue = result.Data.CouponValue,
                    R2SFee = result.Data.R2SFee,
                    DocumentReturn = result.Data.DocumentReturn,
                    DoubleCheck = result.Data.DoubleCheck,
                    CodFee = result.Data.CodFee,
                    PickRemoteAreasFee = result.Data.PickRemoteAreasFee,
                    DeliverRemoteAreasFee = result.Data.DeliverRemoteAreasFee,
                    CodFailedFee = result.Data.CodFailedFee,
                    ReturnAgainFee = result.Data.ReturnAgainFee
                } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping fee");
            return new ShippingFeeResult
            {
                IsSuccess = false,
                Message = $"Error calculating shipping fee: {ex.Message}"
            };
        }
    }

    public async Task<ShippingPreviewResult> PreviewOrderAsync(ShippingOrderRequest request)
    {
        try
        {
            // Validate required fields before mapping
            if (string.IsNullOrWhiteSpace(request.ToName))
            {
                return new ShippingPreviewResult
                {
                    IsSuccess = false,
                    Message = "ToName is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToPhone))
            {
                return new ShippingPreviewResult
                {
                    IsSuccess = false,
                    Message = "ToPhone is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToAddress))
            {
                return new ShippingPreviewResult
                {
                    IsSuccess = false,
                    Message = "ToAddress is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToWardName))
            {
                return new ShippingPreviewResult
                {
                    IsSuccess = false,
                    Message = "ToWardName is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToDistrictName))
            {
                return new ShippingPreviewResult
                {
                    IsSuccess = false,
                    Message = "ToDistrictName is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToProvinceName))
            {
                return new ShippingPreviewResult
                {
                    IsSuccess = false,
                    Message = "ToProvinceName is required"
                };
            }

            var ghnRequest = MapToGHNCreateOrderRequest(request);
            
            // Serialize to JSON to log actual request being sent
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };
            var requestJson = JsonSerializer.Serialize(ghnRequest, jsonOptions);
            _logger.LogInformation("[GHN Preview] Request JSON: {RequestJson}", requestJson);
            
            var httpClient = CreateHttpClient();
            // Use custom options to ensure snake_case and ignore null values
            var response = await httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}{_settings.PreviewPrefix}", 
                ghnRequest,
                new JsonSerializerOptions 
                { 
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
                });

            // Read response body once
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("[GHN Preview] Response Status: {Status}, Body: {Body}", 
                response.StatusCode, responseBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[GHN Preview] Failed with status {Status}: {Body}", 
                    response.StatusCode, responseBody);
                return new ShippingPreviewResult
                {
                    IsSuccess = false,
                    Message = $"GHN API returned {response.StatusCode}: {responseBody}"
                };
            }

            // Parse JSON from the string we already read
            var result = JsonSerializer.Deserialize<GHNPreviewOrderResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (result == null)
            {
                return new ShippingPreviewResult
                {
                    IsSuccess = false,
                    Message = "Failed to parse GHN response"
                };
            }

            if (result.Code != 200)
            {
                return new ShippingPreviewResult
                {
                    IsSuccess = false,
                    Message = result.Message
                };
            }

            return new ShippingPreviewResult
            {
                IsSuccess = true,
                Message = result.Message,
                Data = result.Data != null ? new ShippingPreviewData
                {
                    OrderCode = result.Data.OrderCode,
                    TotalFee = result.Data.TotalFee,
                    ExpectedDeliveryTime = result.Data.ExpectedDeliveryTime,
                    Fee = result.Data.Fee != null ? new ShippingOrderFee
                    {
                        MainService = result.Data.Fee.MainService,
                        Insurance = result.Data.Fee.Insurance,
                        StationDo = result.Data.Fee.StationDo,
                        StationPu = result.Data.Fee.StationPu,
                        Return = result.Data.Fee.Return,
                        R2S = result.Data.Fee.R2S,
                        Coupon = result.Data.Fee.Coupon,
                        CodFailedFee = result.Data.Fee.CodFailedFee
                    } : null
                } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing order");
            return new ShippingPreviewResult
            {
                IsSuccess = false,
                Message = $"Error previewing order: {ex.Message}"
            };
        }
    }

    public async Task<ShippingLeadTimeResult> GetLeadTimeAsync(ShippingLeadTimeRequest request)
    {
        try
        {
            var fromDistrict = request.FromDistrictId > 0 ? request.FromDistrictId : _settings.ShopDistrictId;
            var fromWard = !string.IsNullOrWhiteSpace(request.FromWardCode) ? request.FromWardCode : _settings.ShopWardCode;

            var serviceId = request.ServiceId;
            if (!serviceId.HasValue || serviceId.Value <= 0)
            {
                serviceId = await ResolveServiceIdAsync(fromDistrict, request.ToDistrictId, request.ServiceTypeId);
                if (!serviceId.HasValue)
                {
                    _logger.LogWarning(
                        "[GHN LeadTime] Unable to resolve service_id for FromDistrict {FromDistrict}, ToDistrict {ToDistrict}, ServiceType {ServiceType}",
                        fromDistrict,
                        request.ToDistrictId,
                        request.ServiceTypeId);

                    return new ShippingLeadTimeResult
                    {
                        IsSuccess = false,
                        Message = "GHN service_id not available for the selected route"
                    };
                }
            }

            var ghnRequest = new GHNLeadTimeRequest
            {
                FromDistrictId = fromDistrict,
                FromWardCode = fromWard,
                ToDistrictId = request.ToDistrictId,
                ToWardCode = request.ToWardCode,
                ServiceId = serviceId.Value
            };

            var httpClient = CreateHttpClient();
            _logger.LogInformation("[GHN LeadTime] Sending request {@Request}", ghnRequest);
            var response = await httpClient.PostAsJsonAsync($"{_settings.BaseUrl}{_settings.LeadTimePrefix}", ghnRequest);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GHNLeadTimeResponse>(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            _logger.LogInformation(
                "[GHN LeadTime] Response Code {Code}, Message {Message}, LeadTime {LeadTime}, OrderDate {OrderDate}",
                result?.Code,
                result?.Message,
                result?.Data?.LeadTimeUtc ?? result?.Data?.LeadTimeOrder?.ToEstimateDate,
                result?.Data?.OrderDateUnix);

            if (result == null)
            {
                return new ShippingLeadTimeResult
                {
                    IsSuccess = false,
                    Message = "Failed to parse GHN response"
                };
            }

            if (result.Code != 200)
            {
                _logger.LogWarning(
                    "[GHN LeadTime] Non-success response. Request {@Request}, Response {@Response}",
                    ghnRequest,
                    result);

                return new ShippingLeadTimeResult
                {
                    IsSuccess = false,
                    Message = result.Message
                };
            }

            var leadTimeData = result.Data;
            var normalizedLeadTime = leadTimeData?.LeadTimeUtc
                ?? leadTimeData?.LeadTimeOrder?.ToEstimateDate
                ?? leadTimeData?.LeadTimeOrder?.FromEstimateDate;

            if (leadTimeData == null || (normalizedLeadTime == null && leadTimeData.LeadTimeOrder == null))
            {
                _logger.LogWarning(
                    "[GHN LeadTime] Lead time not returned. Request {@Request}, Response {@Response}",
                    ghnRequest,
                    result);
            }

            return new ShippingLeadTimeResult
            {
                IsSuccess = true,
                Message = result.Message,
                Data = leadTimeData == null
                    ? null
                    : new ShippingLeadTimeData
                {
                        LeadTime = normalizedLeadTime,
                        LeadTimeUnix = leadTimeData.LeadTimeUnix,
                        OrderDateUnix = leadTimeData.OrderDateUnix,
                        EstimateFrom = leadTimeData.LeadTimeOrder?.FromEstimateDate,
                        EstimateTo = leadTimeData.LeadTimeOrder?.ToEstimateDate
                    }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lead time with request {@Request}", request);
            return new ShippingLeadTimeResult
            {
                IsSuccess = false,
                Message = $"Error getting lead time: {ex.Message}"
            };
        }
    }

    public async Task<ShippingOrderResult> CreateOrderAsync(ShippingOrderRequest request)
    {
        try
        {
            // Validate required fields before mapping
            if (string.IsNullOrWhiteSpace(request.ToName))
            {
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = "ToName is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToPhone))
            {
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = "ToPhone is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToAddress))
            {
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = "ToAddress is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToWardName))
            {
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = "ToWardName is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToDistrictName))
            {
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = "ToDistrictName is required"
                };
            }
            if (string.IsNullOrWhiteSpace(request.ToProvinceName))
            {
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = "ToProvinceName is required"
                };
            }

            var ghnRequest = MapToGHNCreateOrderRequest(request);
            
            // Serialize to JSON to log actual request being sent
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };
            var requestJson = JsonSerializer.Serialize(ghnRequest, jsonOptions);
            _logger.LogInformation("[GHN CreateOrder] Request JSON: {RequestJson}", requestJson);
            
            var httpClient = CreateHttpClient();
            // Use custom options to ensure snake_case and ignore null values
            var response = await httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}{_settings.CreatePrefix}", 
                ghnRequest,
                new JsonSerializerOptions 
                { 
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
                });

            // Read response body once
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("[GHN CreateOrder] Response Status: {Status}, Body: {Body}", 
                response.StatusCode, responseBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[GHN CreateOrder] Failed with status {Status}: {Body}", 
                    response.StatusCode, responseBody);
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = $"GHN API returned {response.StatusCode}: {responseBody}"
                };
            }

            // Parse JSON from the string we already read
            var result = JsonSerializer.Deserialize<GHNCreateOrderResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (result == null)
            {
                _logger.LogError("[GHN CreateOrder] Failed to parse response body: {Body}", responseBody);
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = "Failed to parse GHN response"
                };
            }

            if (result.Code != 200)
            {
                _logger.LogWarning("[GHN CreateOrder] Non-success response. Request {@Request}, Response {@Response}", 
                    ghnRequest, result);
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = result.MessageDisplay ?? result.Message
                };
            }

            return new ShippingOrderResult
            {
                IsSuccess = true,
                Message = result.MessageDisplay ?? result.Message,
                Data = result.Data != null ? new ShippingOrderData
                {
                    OrderCode = result.Data.OrderCode,
                    SortCode = result.Data.SortCode,
                    TransType = result.Data.TransType,
                    TotalFee = result.Data.TotalFee,
                    ExpectedDeliveryTime = result.Data.ExpectedDeliveryTime,
                    EstimatedDeliveryDate = result.Data.ExpectedDeliveryTime,
                    Fee = result.Data.Fee != null ? new ShippingOrderFee
                    {
                        MainService = result.Data.Fee.MainService,
                        Insurance = result.Data.Fee.Insurance,
                        StationDo = result.Data.Fee.StationDo,
                        StationPu = result.Data.Fee.StationPu,
                        Return = result.Data.Fee.Return,
                        R2S = result.Data.Fee.R2S,
                        Coupon = result.Data.Fee.Coupon,
                        CodFailedFee = result.Data.Fee.CodFailedFee
                    } : null
                } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipping order");
            return new ShippingOrderResult
            {
                IsSuccess = false,
                Message = $"Error creating shipping order: {ex.Message}"
            };
        }
    }

    public ShippingCallbackResult HandleCallback(object callbackRequest)
    {
        try
        {
            if (callbackRequest is not GHNCallbackRequest ghnCallback)
            {
                return new ShippingCallbackResult
                {
                    IsSuccess = false,
                    Message = "Invalid callback request type"
                };
            }

            // Map GHN status to internal status using helper
            var (status, statusName, _) = ShippingStatusHelper.MapGHNStatus(ghnCallback.Status);

            // Parse Time field (ISO 8601 format: "2021-11-11T03:52:50.158Z")
            DateTime? updatedAt = null;
            if (!string.IsNullOrEmpty(ghnCallback.Time))
            {
                if (DateTime.TryParse(ghnCallback.Time, out var parsedTime))
                {
                    updatedAt = parsedTime;
                }
            }

            // Fallback to current time if Time is not available
            if (!updatedAt.HasValue)
            {
                updatedAt = DateTime.UtcNow;
            }

            return new ShippingCallbackResult
            {
                IsSuccess = true,
                Message = "Callback processed successfully",
                Data = new ShippingCallbackData
                {
                    OrderCode = ghnCallback.OrderCode,
                    ClientOrderCode = ghnCallback.ClientOrderCode,
                    Status = status,
                    StatusName = statusName,
                    UpdatedAt = updatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling callback");
            return new ShippingCallbackResult
            {
                IsSuccess = false,
                Message = $"Error handling callback: {ex.Message}"
            };
        }
    }

    public async Task<ShippingOrderResult> CancelOrderAsync(string orderCode)
    {
        try
        {
            var cancelRequest = new GHNCancelOrderRequest
            {
                OrderCode = orderCode
            };

            var httpClient = CreateHttpClient();
            var response = await httpClient.PostAsJsonAsync($"{_settings.BaseUrl}{_settings.CancelPrefix}", cancelRequest);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GHNCreateOrderResponse>(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (result == null)
            {
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = "Failed to parse GHN cancel response"
                };
            }

            if (result.Code != 200)
            {
                return new ShippingOrderResult
                {
                    IsSuccess = false,
                    Message = result.Message
                };
            }

            return new ShippingOrderResult
            {
                IsSuccess = true,
                Message = result.MessageDisplay ?? result.Message,
                Data = result.Data != null ? new ShippingOrderData
                {
                    OrderCode = result.Data.OrderCode,
                    SortCode = result.Data.SortCode,
                    TransType = result.Data.TransType,
                    TotalFee = result.Data.TotalFee,
                    ExpectedDeliveryTime = result.Data.ExpectedDeliveryTime,
                    EstimatedDeliveryDate = result.Data.ExpectedDeliveryTime
                } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling shipping order: {OrderCode}", orderCode);
            return new ShippingOrderResult
            {
                IsSuccess = false,
                Message = $"Error canceling shipping order: {ex.Message}"
            };
        }
    }

    public string GetProviderName()
    {
        return "GHN";
    }

    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Token", _settings.Token);
        client.DefaultRequestHeaders.Add("ShopId", _settings.ShopId);
        return client;
    }

    private async Task<int?> ResolveServiceIdAsync(int fromDistrictId, int toDistrictId, int preferredServiceTypeId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.AvailableServicesPrefix))
            {
                _logger.LogWarning("[GHN AvailableServices] Prefix not configured.");
                return null;
            }

            if (!int.TryParse(_settings.ShopId, out var shopId))
            {
                _logger.LogWarning("[GHN AvailableServices] ShopId is not a valid integer: {ShopId}", _settings.ShopId);
                return null;
            }

            var request = new GHNAvailableServiceRequest
            {
                ShopId = shopId,
                FromDistrict = fromDistrictId > 0 ? fromDistrictId : _settings.ShopDistrictId,
                ToDistrict = toDistrictId
            };

            var httpClient = CreateHttpClient();
            _logger.LogInformation("[GHN AvailableServices] Sending request {@Request}", request);
            var response = await httpClient.PostAsJsonAsync($"{_settings.BaseUrl}{_settings.AvailableServicesPrefix}", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GHNAvailableServiceResponse>(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (result == null || result.Code != 200 || result.Data == null || result.Data.Count == 0)
            {
                _logger.LogWarning(
                    "[GHN AvailableServices] Unexpected response. Request {@Request}, Response {@Response}",
                    request,
                    result);
                return null;
            }

            var preferredService = result.Data.FirstOrDefault(x => x.ServiceTypeId == preferredServiceTypeId);
            var resolvedId = preferredService?.ServiceId ?? result.Data.First().ServiceId;

            _logger.LogInformation(
                "[GHN AvailableServices] Resolved service_id {ServiceId} (preferred type {PreferredType})",
                resolvedId,
                preferredServiceTypeId);

            return resolvedId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GHN AvailableServices] Error resolving service_id for FromDistrict {FromDistrict}, ToDistrict {ToDistrict}",
                fromDistrictId,
                toDistrictId);
            return null;
        }
    }

    private GHNCreateOrderRequest MapToGHNCreateOrderRequest(ShippingOrderRequest request)
    {
        // Only set PickupTime if it's a valid Unix timestamp (must be > 0)
        // GHN expects Unix timestamp in seconds, not milliseconds
        long? pickupTime = null;
        if (request.PickupTime.HasValue && request.PickupTime.Value > 0)
        {
            // If value is in milliseconds (too large), convert to seconds
            var value = request.PickupTime.Value;
            if (value > 9999999999) // Likely milliseconds (10 digits)
            {
                pickupTime = value / 1000;
            }
            else
            {
                pickupTime = value;
            }
        }

        return new GHNCreateOrderRequest
        {
            PaymentTypeId = request.PaymentTypeId,
            Note = request.Note,
            RequiredNote = request.RequiredNote,
            ReturnPhone = request.ReturnPhone,
            ReturnAddress = request.ReturnAddress,
            ReturnDistrictId = request.ReturnDistrictId,
            ReturnWardCode = request.ReturnWardCode,
            ClientOrderCode = request.ClientOrderCode,
            FromName = request.FromName ?? DefaultFromName,
            FromPhone = request.FromPhone ?? DefaultFromPhone,
            FromAddress = request.FromAddress ?? DefaultFromAddress,
            FromWardName = request.FromWardName ?? DefaultFromWardName,
            FromDistrictName = request.FromDistrictName ?? DefaultFromDistrictName,
            FromProvinceName = request.FromProvinceName ?? DefaultFromProvinceName,
            ToName = request.ToName,
            ToPhone = request.ToPhone,
            ToAddress = request.ToAddress,
            ToWardName = request.ToWardName,
            ToDistrictName = request.ToDistrictName,
            ToProvinceName = request.ToProvinceName,
            CodAmount = request.CodAmount,
            Content = request.Note,
            Length = request.Length > 0 ? request.Length : DefaultLength,
            Width = request.Width > 0 ? request.Width : DefaultWidth,
            Height = request.Height > 0 ? request.Height : DefaultHeight,
            Weight = request.Weight > 0 ? request.Weight : DefaultWeight,
            CodFailedAmount = request.CodFailedAmount,
            PickStationId = request.PickStationId,
            DeliverStationId = request.DeliverStationId,
            InsuranceValue = request.InsuranceValue,
            ServiceTypeId = request.ServiceTypeId,
            Coupon = request.Coupon,
            PickupTime = pickupTime, // Only set if valid
            PickShift = request.PickShift,
            // Include Items if provided (GHN accepts items for both service_type_id = 2 and 5)
            // For service_type_id = 2, GHN uses root level length/width/height/weight for calculation
            // For service_type_id = 5, GHN uses items[].length/width/height/weight for calculation
            Items = request.Items != null && request.Items.Any()
                ? request.Items.Select(item => new GHNOrderItem
                {
                    Name = item.Name,
                    Code = item.Code,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Length = item.Length > 0 ? item.Length : DefaultLength,
                    Width = item.Width > 0 ? item.Width : DefaultWidth,
                    Height = item.Height > 0 ? item.Height : DefaultHeight,
                    Weight = item.Weight > 0 ? item.Weight : DefaultWeight,
                    Category = item.Category != null ? new GHNOrderItemCategory
                    {
                        Level1 = item.Category.Level1,
                        Level2 = item.Category.Level2,
                        Level3 = item.Category.Level3
                    } : null
                }).ToList()
                : null
        };
    }

    // MapGHNStatus method removed - now using ShippingStatusHelper.MapGHNStatus
    // This ensures consistent status mapping across the application
}

