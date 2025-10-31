using System.Text.Json.Serialization;
using NekoViBE.Application.Common.Enums;

namespace NekoViBE.Application.Common.DTOs.Auth;

public class ResetPasswordRequest
{
    [JsonPropertyName("contact")]
    public string Contact { get; set; }
    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; }
    [JsonPropertyName("otpSentChannel")]
    public NotificationChannelEnum OtpSentChannel { get; set; } = NotificationChannelEnum.Email;
}
