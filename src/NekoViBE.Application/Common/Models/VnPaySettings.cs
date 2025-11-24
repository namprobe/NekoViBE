namespace NekoViBE.Application.Common.Models;

public class VnPaySettings
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public int TimeOut { get; set; } = 900;
}