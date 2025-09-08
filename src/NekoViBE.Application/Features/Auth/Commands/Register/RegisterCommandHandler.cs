using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Common;
using System.Transactions;
using NekoViBE.Domain.Enums;
using NekoViBE.Application.Common.Helpers;

namespace NekoViBE.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
{
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IOtpCacheService _otpCacheService;
    private readonly INotificationFactory _notificationFactory;

    public RegisterCommandHandler(
        ILogger<RegisterCommandHandler> logger, 
        IOtpCacheService otpCacheService,
        INotificationFactory notificationFactory)
    {
        _logger = logger;
        _otpCacheService = otpCacheService;
        _notificationFactory = notificationFactory;
    }
    public async Task<Result> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        try
        {   
            // Determine contact based on channel
            var channel = command.Request.OtpSentChannel ?? NotificationChannelEnum.Email;
            var contact = channel == NotificationChannelEnum.Email ? 
                command.Request.Email : command.Request.PhoneNumber;

            if (string.IsNullOrEmpty(contact))
            {
                return Result.Failure("Contact information is required", ErrorCodeEnum.ValidationFailed);
            }

            // Generate and store OTP
            var otpCode = _otpCacheService.GenerateAndStoreOtp(
                contact, 
                OtpTypeEnum.Registration, 
                command.Request, 
                channel);

            // Build notification using static template helper
            var notification = NotificationTemplateHelper.BuildOtpNotification(
                contact, 
                otpCode, 
                OtpTypeEnum.Registration, 
                channel, 
                command.Request,
                _otpCacheService.ExpirationMinutes);

            // Get notification sender based on channel
            var notificationSender = _notificationFactory.GetSender(channel);

            // Create recipient info
            var recipient = new RecipientInfo
            {
                Email = channel == NotificationChannelEnum.Email ? contact : null,
                PhoneNumber = channel == NotificationChannelEnum.Email ? null : contact,
                FullName = $"{command.Request.FirstName} {command.Request.LastName}".Trim()
            };

            // Send notification
            var sendResult = await notificationSender.SendNotificationAsync(notification, recipient);

            if (sendResult.ChannelResults.Any(cr => cr.Success))
            {
                _logger.LogInformation("OTP sent successfully to {Contact} via {Channel} for registration", 
                    contact, channel);
                return Result.Success($"Registration initiated. Please verify the OTP sent to your {channel.ToString().ToLower()} to complete the registration process.");
            }
            else
            {
                _logger.LogError("Failed to send OTP to {Contact} via {Channel}", contact, channel);
                return Result.Failure("Failed to send verification code. Please try again.", ErrorCodeEnum.InternalError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration process");
            return Result.Failure("Error during registration process", ErrorCodeEnum.InternalError);
        }
    }
}