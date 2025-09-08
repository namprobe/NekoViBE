using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings, 
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public Task<List<NotificationSendResult>> SendMultiCastAsync(NotificationRequest message, List<RecipientInfo> recipients)
    {
        throw new NotImplementedException();
    }

    public async Task<NotificationSendResult> SendNotificationAsync(NotificationRequest message, RecipientInfo recipient)
    {
        try
        {
            // Use the pre-built HTML content from the notification request
            string htmlContent = message.HtmlContent ?? message.Content;

            // Send email using SMTP client
            using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
            {
                client.Credentials = new System.Net.NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                client.EnableSsl = _emailSettings.EnableSsl;
                client.Timeout = 30000; // 30 seconds timeout

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = message.Subject,
                    Body = htmlContent,
                    IsBodyHtml = true,
                };

                if (string.IsNullOrEmpty(recipient.Email))
                {
                    throw new ArgumentException("Recipient email cannot be null or empty", nameof(recipient.Email));
                }

                mailMessage.To.Add(new MailAddress(recipient.Email, recipient.FullName ?? string.Empty));

                await client.SendMailAsync(mailMessage);
                
                _logger.LogInformation("Email sent successfully to {Email} with subject {Subject}", recipient.Email, message.Subject);
                
                return new NotificationSendResult
                {
                    UserId = recipient.UserId,
                    Email = recipient.Email,
                    FullName = recipient.FullName,
                    ChannelResults = new List<NotificationChannelResult>
                    {
                        new NotificationChannelResult
                        {
                            Channel = NotificationChannelEnum.Email,
                            Success = true,
                            Message = "Email sent successfully",
                            Timestamp = DateTime.UtcNow
                        }
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} with subject {Subject}", recipient.Email, message.Subject);
            return new NotificationSendResult
            {
                UserId = recipient.UserId,
                Email = recipient.Email,
                FullName = recipient.FullName,
                ChannelResults = new List<NotificationChannelResult>
                {
                    new NotificationChannelResult
                    {
                        Channel = NotificationChannelEnum.Email,
                        Success = false,
                        Message = "Failed to send email",
                        ErrorMessage = ex.Message,
                        Timestamp = DateTime.UtcNow
                    }
                }
            };
        }
    }

} 