using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Infrastructure.Services
{
    public class OtpCacheService : IOtpCacheService
    {
        private readonly ILogger<OtpCacheService> _logger;
        private static readonly ConcurrentDictionary<string, OtpCacheItem> _cache = new();
        private readonly OtpSettings _otpSettings;
        //private readonly Random _random = new();

        public int ExpirationMinutes => _otpSettings.ExpirationMinutes;

        public OtpCacheService(ILogger<OtpCacheService> logger, IOptions<OtpSettings> otpSettings)
        {
            _logger = logger;
            _otpSettings = otpSettings.Value;
        }

        // Implementation of the OTP cache service
        public int CleanUpExpriredOtp()
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiredKeys = _cache.Where(exk => exk.Value.ExpiresAt <= now).Select(exk => exk.Key).ToList();
                var removedCount = 0;
                foreach (var key in expiredKeys)
                {
                    _cache.TryRemove(key, out _);
                    removedCount++;
                    _logger.LogInformation("Removed expired OTP cache item with key {Key}", key);
                }
                return removedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup of expired OTP cache items");
                return 0;
            }
        }

        public void ClearAllCache()
        {
            _cache.Clear();
            _logger.LogInformation("Cleared all OTP cache items");
        }

        public string GenerateAndStoreOtp(string contact, OtpTypeEnum type, object userData, NotificationChannelEnum channel = NotificationChannelEnum.Email)
        {
            try
            {
                //1. Generate OTP
                var otpCode = GenerateSecureOtp(_otpSettings.Length);
                //2. Generate Cache Key
                var cacheKey = GenerateCacheKey(contact, type);
                
                //4. Create OtpCacheItem
                var cacheItem = new OtpCacheItem
                {
                    Contact = contact,
                    OtpCode = otpCode,
                    Channel = channel,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpirationMinutes),
                    CreatedAt = DateTime.UtcNow,
                    AttemptCount = 0,
                    MaxAttempts = _otpSettings.MaxAttempts,
                    IsVerified = false,
                    Type = type,
                    userData = userData
                };
                //5. Store in cache with override if key existed
                _cache.AddOrUpdate(cacheKey, cacheItem, (key, existingItem) => cacheItem);
                // Clean up expired items (fire-and-forget)
                _ = Task.Run(CleanUpExpriredOtp);
                _logger.LogInformation("Generated and stored OTP for {Contact} with type {Type}, expires at {ExpiresAt}",
                    contact, type, cacheItem.ExpiresAt);
                return otpCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating or storing OTP for {Contact} with type {Type}", contact, type);
                throw;
            }
        }

        //helper method to generate OTP
        private string GenerateSecureOtp(int length)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(bytes);
                int digit = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 10;
                chars[i] = (char)('0' + digit); // chuyển số thành ký tự trực tiếp
            }

            return new string(chars);
        }

        //helper method to generate cache key
        private string GenerateCacheKey(string contact, OtpTypeEnum type)
        {
            return $"otp_{type.ToString().ToLowerInvariant()}_{contact.ToLowerInvariant()}";
        }
        public int GetActiveCacheCount()
        {
            CleanUpExpriredOtp();
            return _cache.Count();
        }

        public OtpCacheItem? GetOtpData(string contact, OtpTypeEnum type)
        {
            var cacheKey = GenerateCacheKey(contact, type);
            if (_cache.TryGetValue(cacheKey, out var cacheItem))
            {
                if (cacheItem.ExpiresAt > DateTime.UtcNow)
                {
                    // Item is still valid
                    return cacheItem;
                }
                else
                {
                    //remove expired item
                    _cache.TryRemove(cacheKey, out _);
                    _logger.LogInformation("OTP for {Contact} with type {Type} has expired and was removed from cache", contact, type);
                }
            }
            return null; // Not found or expired;
        }

        public int GetRemainingAttempts(string contact, OtpTypeEnum type)
        {
            var cacheItem = GetOtpData(contact, type);
            if (cacheItem == null)
                return 0;

            return Math.Max(0, cacheItem.MaxAttempts - cacheItem.AttemptCount);
        }

        public TimeSpan GetRemainingTime(string contact, OtpTypeEnum type)
        {
            var cacheItem = GetOtpData(contact, type);
            if (cacheItem == null || cacheItem.ExpiresAt <= DateTime.UtcNow)
                return TimeSpan.Zero;

            return cacheItem.ExpiresAt - DateTime.UtcNow;

        }

        public bool IsOtpExpired(string contact, OtpTypeEnum type)
        {
            var cacheItem = GetOtpData(contact, type);
            return cacheItem == null || cacheItem.ExpiresAt <= DateTime.UtcNow;
        }

        public void RemoveOtp(string contact, OtpTypeEnum type)
        {
            var cacheKey = GenerateCacheKey(contact, type);
            _cache.TryRemove(cacheKey, out _);
            _logger.LogInformation("Removing OTP for {Contact} with type {Type} from cache", contact, type);
        }

        public OtpResult VerifyOtp(string contact, string otpCode, OtpTypeEnum type, NotificationChannelEnum channel = NotificationChannelEnum.Email)
        {
            try
            {
                //1. Generate Cache Key
                var cacheKey = GenerateCacheKey(contact, type);
                //2. Try get cache item
                var cacheItem = GetOtpData(contact, type);
                //3. If valid cache item found, increase attempt count and update
                if (cacheItem == null)
                {
                    _logger.LogWarning("No valid OTP found for {Contact} with type {Type}", contact, type);
                    return new OtpResult
                    {
                        Success = false,
                        Message = "No valid OTP found or OTP has expired."
                    };
                }

                if (cacheItem.Channel != channel)
                {
                    _logger.LogWarning("OTP channel mismatch for {Contact} with type {Type}. Expected {ExpectedChannel}, got {ActualChannel}",
                        contact, type, cacheItem.Channel, channel);
                    return new OtpResult
                    {
                        Success = false,
                        Message = "OTP channel mismatch."
                    };
                }

                if (cacheItem.IsVerified)
                {
                    _logger.LogWarning("OTP for {Contact} with type {Type} has already been verified", contact, type);
                    RemoveOtp(contact, type);
                    return new OtpResult
                    {
                        Success = false,
                        Message = "OTP has already been verified."
                    };
                }

                // if (cacheItem.ExpiresAt <= DateTime.UtcNow)
                // {
                //     RemoveOtp(contact, type);
                //     _logger.LogWarning("OTP for {Contact} with type {Type} has expired", contact, type);
                //     return false;
                // }

                cacheItem.AttemptCount++;
                _cache.AddOrUpdate(cacheKey, cacheItem, (key, existingItem) => cacheItem);

                //4. check max attempts
                if (cacheItem.AttemptCount > cacheItem.MaxAttempts)
                {
                    _logger.LogWarning("Max OTP attempts exceeded for {Contact} with type {Type}", contact, type);
                    RemoveOtp(contact, type);
                    return new OtpResult
                    {
                        Success = false,
                        Message = "Max OTP attempts exceeded."
                    };
                }

                //5. If check max attempts passed, verify OTP code
                bool isValid = cacheItem.OtpCode.Equals(otpCode, StringComparison.Ordinal);

                //6. if verified, mark as verified and update cache item (do not remove yet for cqrs use case)
                if (!isValid)
                {
                    _logger.LogWarning("Invalid OTP code provided for {Contact} with type {Type}. Attempt {AttemptCount}/{MaxAttempts}",
                        contact, type, cacheItem.AttemptCount, cacheItem.MaxAttempts);
                    return new OtpResult
                    {
                        Success = false,
                        Message = "Invalid OTP code.",
                        RemainingAttempts = GetRemainingAttempts(contact, type),
                        ExpiresIn = GetRemainingTime(contact, type)
                    };
                }
                cacheItem.IsVerified = true;
                _cache.AddOrUpdate(cacheKey, cacheItem, (key, existingItem) => cacheItem);
                _logger.LogInformation("OTP verified successfully for {Contact} with type {Type}", contact, type);
                return new OtpResult
                {
                    Success = true,
                    Message = "OTP verified successfully.",
                    UserData = cacheItem.userData,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {Contact} with type {Type}", contact, type);
                return new OtpResult
                {
                    Success = false,
                    Message = "Error verifying OTP."
                };
            }
        }
    }
}