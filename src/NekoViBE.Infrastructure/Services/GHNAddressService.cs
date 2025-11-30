// using System;
// using System.Net.Http.Json;
// using System.Text.Json;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using NekoViBE.Application.Common.Models.GHN;
// using NekoViBE.Domain.Entities;
// using System.Text.Json.Serialization;

// namespace NekoViBE.Infrastructure.Services;

// /// <summary>
// /// Service để lấy và seed dữ liệu địa chỉ từ GHN API
// /// </summary>
// public class GHNAddressService
// {
//     private readonly HttpClient _httpClient;
//     private readonly ILogger<GHNAddressService> _logger;
//     private readonly GHNSettings _settings;
//     private readonly JsonSerializerOptions _jsonOptions;

//     public GHNAddressService(
//         IHttpClientFactory httpClientFactory,
//         ILogger<GHNAddressService> logger,
//         IOptions<GHNSettings> ghnSettings)
//     {
//         _httpClient = httpClientFactory.CreateClient();
//         _logger = logger;
//         _settings = ghnSettings.Value;
//         var normalizedBaseUrl = _settings.BaseUrl.TrimEnd('/') + "/";
//         _httpClient.BaseAddress = new Uri(normalizedBaseUrl);
//         _httpClient.DefaultRequestHeaders.Add("Token", _settings.Token);
//         _jsonOptions = new JsonSerializerOptions
//         {
//             PropertyNameCaseInsensitive = true,
//             NumberHandling = JsonNumberHandling.AllowReadingFromString
//         };
//     }

//     /// <summary>
//     /// Lấy danh sách tỉnh/thành từ GHN API
//     /// </summary>
//     public async Task<List<ProvinceResponse>> GetProvincesAsync()
//     {
//         try
//         {
//             var response = await _httpClient.GetAsync("master-data/province");
//             response.EnsureSuccessStatusCode();

//             var result = await response.Content.ReadFromJsonAsync<GHNProvinceResponse>(_jsonOptions);

//             if (result?.Code == 200 && result.Data != null)
//             {
//                 return result.Data;
//             }

//             _logger.LogError("Failed to get provinces: {Message}", result?.Message);
//             return new List<ProvinceResponse>();
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error getting provinces from GHN API");
//             return new List<ProvinceResponse>();
//         }
//     }

//     /// <summary>
//     /// Lấy danh sách quận/huyện từ GHN API
//     /// </summary>
//     public async Task<List<DistrictResponse>> GetDistrictsAsync(int provinceId)
//     {
//         try
//         {
//             var requestBody = new { province_id = provinceId };
//             var response = await _httpClient.PostAsJsonAsync("master-data/district", requestBody);
//             response.EnsureSuccessStatusCode();

//             var result = await response.Content.ReadFromJsonAsync<GHNDistrictResponse>(_jsonOptions);

//             if (result?.Code == 200 && result.Data != null)
//             {
//                 return result.Data;
//             }

//             _logger.LogError("Failed to get districts for province {ProvinceId}: {Message}", provinceId, result?.Message);
//             return new List<DistrictResponse>();
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error getting districts from GHN API for province {ProvinceId}", provinceId);
//             return new List<DistrictResponse>();
//         }
//     }

//     /// <summary>
//     /// Lấy danh sách phường/xã từ GHN API
//     /// </summary>
//     public async Task<List<WardResponse>> GetWardsAsync(int districtId)
//     {
//         try
//         {
//             var requestBody = new { district_id = districtId };
//             var response = await _httpClient.PostAsJsonAsync("master-data/ward?district_id", requestBody);
//             response.EnsureSuccessStatusCode();

//             var result = await response.Content.ReadFromJsonAsync<GHNWardResponse>(_jsonOptions);

//             if (result?.Code == 200 && result.Data != null)
//             {
//                 return result.Data;
//             }

//             _logger.LogError("Failed to get wards for district {DistrictId}: {Message}", districtId, result?.Message);
//             return new List<WardResponse>();
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error getting wards from GHN API for district {DistrictId}", districtId);
//             return new List<WardResponse>();
//         }
//     }
// }

// // Response models
// public class GHNProvinceResponse
// {
//     public int Code { get; set; }
//     public string Message { get; set; } = string.Empty;
//     public List<ProvinceResponse>? Data { get; set; }
// }

// public class ProvinceResponse
// {
//     public int ProvinceId { get; set; }
//     public string ProvinceName { get; set; } = string.Empty;
//     public int CountryId { get; set; }
//     public int Code { get; set; }
//     public List<string>? NameExtension { get; set; }
//     public int IsEnable { get; set; }
//     public int RegionId { get; set; }
//     [JsonConverter(typeof(FlexibleBooleanStringConverter))]
//     public string CanUpdateCod { get; set; } = "false";
//     public int Status { get; set; }
// }

// public class GHNDistrictResponse
// {
//     public int Code { get; set; }
//     public string Message { get; set; } = string.Empty;
//     public List<DistrictResponse>? Data { get; set; }
// }

// public class DistrictResponse
// {
//     public int DistrictId { get; set; }
//     public int ProvinceId { get; set; }
//     public string DistrictName { get; set; } = string.Empty;
//     public int Code { get; set; }
//     public int Type { get; set; }
//     public int SupportType { get; set; }
//     public List<string>? NameExtension { get; set; }
//     public int IsEnable { get; set; }
//     [JsonConverter(typeof(FlexibleBooleanStringConverter))]
//     public string CanUpdateCod { get; set; } = "false";
//     public int Status { get; set; }
// }

// public class GHNWardResponse
// {
//     public int Code { get; set; }
//     public string Message { get; set; } = string.Empty;
//     public List<WardResponse>? Data { get; set; }
// }

// public class WardResponse
// {
//     public string WardCode { get; set; } = string.Empty;
//     public int DistrictId { get; set; }
//     public string WardName { get; set; } = string.Empty;
//     public List<string>? NameExtension { get; set; }
//     [JsonConverter(typeof(FlexibleBooleanStringConverter))]
//     public string CanUpdateCod { get; set; } = "false";
//     public int SupportType { get; set; }
//     public int Status { get; set; }
// }

// internal sealed class FlexibleBooleanStringConverter : JsonConverter<string>
// {
//     public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//     {
//         return reader.TokenType switch
//         {
//             JsonTokenType.String => reader.GetString() ?? string.Empty,
//             JsonTokenType.True => "true",
//             JsonTokenType.False => "false",
//             _ => throw new JsonException($"Unexpected token {reader.TokenType} when parsing boolean string.")
//         };
//     }

//     public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
//     {
//         if (bool.TryParse(value, out var booleanValue))
//         {
//             writer.WriteBooleanValue(booleanValue);
//             return;
//         }

//         writer.WriteStringValue(value);
//     }
// }

