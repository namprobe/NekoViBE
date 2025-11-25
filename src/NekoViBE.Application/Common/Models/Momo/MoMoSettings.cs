namespace NekoViBE.Application.Common.Models.Momo;

public class MoMoSettings
{
    public string MomoPartnerCode { get; set; } = string.Empty;
    public string MomoPartnerName { get; set; } = string.Empty;
    public string MomoStoreId { get; set; } = string.Empty;
    public string MomoAccessToken { get; set; } = string.Empty;
    public string MomoSecretKey { get; set; } = string.Empty;
    public string MomoApiEndpoint { get; set; } = string.Empty;
    public string MomoIpnUrl { get; set; } = string.Empty;
    public string MomoRedirectUrl { get; set; } = string.Empty;
    public string MomoMobileRedirectUrl { get; set; } = string.Empty;
    public string? MomoIpnWhitelist { get; set; }
}