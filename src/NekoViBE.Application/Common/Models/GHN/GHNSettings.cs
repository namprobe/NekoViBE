namespace NekoViBE.Application.Common.Models.GHN;

public class GHNSettings
{
    public string Token { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string CreatePrefix { get; set; } = string.Empty;
    public string DetailPrefixByClientCode { get; set; } = string.Empty;
    public string DatePrefix { get; set; } = string.Empty;
    public string PreviewPrefix { get; set; } = string.Empty;
    public string LeadTimePrefix { get; set; } = string.Empty;
    public string UpdatePrefix { get; set; } = string.Empty;
    public string CancelPrefix { get; set; } = string.Empty;
    public string ReturnPrefix { get; set; } = string.Empty;
    public string GenTokenPrefix { get; set; } = string.Empty;
    public string DetailPrefix { get; set; } = string.Empty;
    public string StoringPrefix { get; set; } = string.Empty;
    public string UpdateCODPrefix { get; set; } = string.Empty;
    public string GetStationPrefix { get; set; } = string.Empty;
    public string GetFeePrefix { get; set; } = string.Empty;
    public string AvailableServicesPrefix { get; set; } = string.Empty;
    public string CallbackOrder { get; set; } = string.Empty;
    public int ShopProvinceId { get; set; }
    public int ShopDistrictId { get; set; }
    public string ShopWardCode { get; set; } = null!;
}

