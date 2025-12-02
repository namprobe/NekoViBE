namespace NekoViBE.Infrastructure.Utils;

/// <summary>
/// Helper class để xử lý chuyển đổi thời gian giữa UTC và GMT+7 (timezone của VNPay)
/// </summary>
public static class VnPayDateTimeHelper
{
    private static readonly TimeZoneInfo VietnamTimeZone = GetVietnamTimeZone();

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try
        {
            // Windows timezone ID
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch
        {
            try
            {
                // Linux/Mac timezone ID
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch
            {
                // Fallback: tạo timezone GMT+7 manually
                return TimeZoneInfo.CreateCustomTimeZone(
                    "Vietnam Standard Time",
                    TimeSpan.FromHours(7),
                    "Vietnam Standard Time",
                    "Vietnam Standard Time");
            }
        }
    }

    /// <summary>
    /// Chuyển đổi UTC DateTime sang GMT+7 và format theo định dạng VNPay yêu cầu (yyyyMMddHHmmss)
    /// </summary>
    /// <param name="utcDateTime">UTC DateTime cần chuyển đổi</param>
    /// <returns>String định dạng yyyyMMddHHmmss theo timezone GMT+7</returns>
    public static string ConvertUtcToVietnamTimeString(DateTime utcDateTime)
    {
        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
        return vietnamTime.ToString("yyyyMMddHHmmss");
    }

    /// <summary>
    /// Chuyển đổi UTC DateTime sang GMT+7
    /// </summary>
    /// <param name="utcDateTime">UTC DateTime cần chuyển đổi</param>
    /// <returns>DateTime theo timezone GMT+7</returns>
    public static DateTime ConvertUtcToVietnamTime(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
    }

    /// <summary>
    /// Parse string định dạng VNPay (yyyyMMddHHmmss) từ GMT+7 và chuyển về UTC
    /// </summary>
    /// <param name="vietnamTimeString">String định dạng yyyyMMddHHmmss từ VNPay (GMT+7)</param>
    /// <returns>UTC DateTime</returns>
    public static DateTime ParseVietnamTimeStringToUtc(string vietnamTimeString)
    {
        if (string.IsNullOrWhiteSpace(vietnamTimeString))
        {
            return DateTime.UtcNow;
        }

        // Parse string theo format VNPay: yyyyMMddHHmmss
        if (DateTime.TryParseExact(vietnamTimeString, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var vietnamTime))
        {
            // Chuyển từ GMT+7 về UTC
            return TimeZoneInfo.ConvertTimeToUtc(vietnamTime, VietnamTimeZone);
        }

        throw new ArgumentException($"Invalid VNPay datetime format: {vietnamTimeString}. Expected format: yyyyMMddHHmmss");
    }

    /// <summary>
    /// Lấy thời gian hiện tại theo GMT+7 và format theo định dạng VNPay
    /// </summary>
    /// <returns>String định dạng yyyyMMddHHmmss theo timezone GMT+7</returns>
    public static string GetCurrentVietnamTimeString()
    {
        return ConvertUtcToVietnamTimeString(DateTime.UtcNow);
    }

    /// <summary>
    /// Tính thời gian hết hạn (UTC + minutes) và chuyển sang GMT+7 format
    /// </summary>
    /// <param name="minutesFromNow">Số phút từ bây giờ</param>
    /// <returns>String định dạng yyyyMMddHHmmss theo timezone GMT+7</returns>
    public static string GetExpireTimeString(int minutesFromNow = 15)
    {
        var expireUtcTime = DateTime.UtcNow.AddMinutes(minutesFromNow);
        return ConvertUtcToVietnamTimeString(expireUtcTime);
    }
}

