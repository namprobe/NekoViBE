using System.Text.Json.Serialization;
using NekoViBE.Application.Common.Enums;

namespace NekoViBE.Application.Common.DTOs.Auth;

public class VerifyOtpRequest
{
    public string Contact { get; set; } = string.Empty;
   
    public string Otp { get; set; } = string.Empty;
   
    public NotificationChannelEnum OtpSentChannel { get; set; } = NotificationChannelEnum.Email;
    
    public OtpTypeEnum OtpType { get; set; } = OtpTypeEnum.Registration;
}