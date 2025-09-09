using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Helpers;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IOtpCacheService _otpCacheService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    private readonly INotificationFactory _notificationFactory;
    public ResetPasswordCommandHandler(ILogger<ResetPasswordCommandHandler> logger, IOtpCacheService otpCacheService,
     IIdentityService identityService, INotificationFactory notificationFactory)
    {
        _otpCacheService = otpCacheService;
        _identityService = identityService;
        _logger = logger;
        _notificationFactory = notificationFactory;
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // No need to check user existence here - will be checked when OTP is verified
            // This reduces database queries and improves performance
            Expression<Func<AppUser, bool>> expression = command.Request.OtpSentChannel switch
            {
                NotificationChannelEnum.Email => x => x.Email == command.Request.Contact,
                NotificationChannelEnum.SMS => x => x.PhoneNumber == command.Request.Contact,
                _ => throw new ArgumentException("Invalid notification channel.")
            };
            var user = await _identityService.GetUserByFirstOrDefaultAsync(expression);
            if (user == null)
            {
                return Result.Failure("User not found", ErrorCodeEnum.NotFound);
            }
            var passwordHash = _identityService.HashPassword(command.Request.NewPassword);

            //create cache for otp with rate limiting check
            string otp;
            try
            {
                otp = _otpCacheService.GenerateAndStoreOtp(command.Request.Contact, OtpTypeEnum.PasswordReset, passwordHash, command.Request.OtpSentChannel);
            }
            catch (InvalidOperationException ex)
            {
                // Rate limiting error - return user-friendly message
                return Result.Failure(ex.Message, ErrorCodeEnum.TooManyRequests);
            }

            
            //build notification body
            var recipient = new RecipientInfo
            {
                Email = command.Request.OtpSentChannel == NotificationChannelEnum.Email ? command.Request.Contact : null,
                PhoneNumber = command.Request.OtpSentChannel == NotificationChannelEnum.SMS ? command.Request.Contact : null,
                FullName = user.FirstName + " " + user.LastName
            };

            // Build minimal notification; EmailService will render by template
            var notification = new NotificationRequest
            {
                To = command.Request.Contact,
                Template = NotificationTemplateEnums.Otp,
                TemplateData = new Dictionary<string, object>
                {
                    ["otpCode"] = otp,
                    ["otpType"] = OtpTypeEnum.PasswordReset.ToString(),
                }
            };
            //send notification
            var notificationSender = _notificationFactory.GetSender(command.Request.OtpSentChannel);
            var sendResult = await notificationSender.SendNotificationAsync(notification, recipient);
            if (sendResult.ChannelResults.Any(cr => cr.Success))
            {
                _logger.LogInformation("OTP sent successfully to {Contact} via {Channel} for registration", 
                    command.Request.Contact, command.Request.OtpSentChannel.ToString().ToLower());
                return Result.Success($"Registration initiated. Please verify the OTP sent to your {command.Request.OtpSentChannel.ToString().ToLower()}) to complete the registration process.");
            }
            else
            {
                    _logger.LogError("Failed to send OTP to {Contact} via {Channel}", command.Request.Contact, command.Request.OtpSentChannel.ToString().ToLower());
                    return Result.Failure("Failed to send verification code. Please try again.", ErrorCodeEnum.InternalError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reset password process");
            return Result.Failure("Error during reset password process",  ErrorCodeEnum.InternalError);
        }
    }
}