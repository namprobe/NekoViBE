using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.DTOs.Auth;
public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("grantType")]
    public GrantTypeEnum GrantType { get; set; } = GrantTypeEnum.Password;
}