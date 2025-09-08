using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Common.Interfaces;

public interface IOtpCacheService
{
    // Core OTP Operations
    string GenerateAndStoreOtp(string contact, OtpTypeEnum type, object userData, NotificationChannelEnum channel = NotificationChannelEnum.Email);
    OtpCacheItem? GetOtpData(string contact, OtpTypeEnum type);
    OtpResult VerifyOtp(string contact, string otpCode, OtpTypeEnum type, NotificationChannelEnum channel = NotificationChannelEnum.Email);
    void RemoveOtp(string contact, OtpTypeEnum type);

    //Utility methods
    int GetRemainingAttempts(string contact, OtpTypeEnum type);
    bool IsOtpExpired(string contact, OtpTypeEnum type);
    TimeSpan GetRemainingTime(string contact, OtpTypeEnum type);

    // Management Operations
    int CleanUpExpriredOtp();
    int GetActiveCacheCount();
    void ClearAllCache();

    // Settings
    int ExpirationMinutes { get; }
}