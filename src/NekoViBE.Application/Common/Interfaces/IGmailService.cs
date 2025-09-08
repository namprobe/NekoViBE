using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Common.Interfaces;

/// <summary>
/// Gmail service interface for sending emails using Google Gmail API
/// </summary>
public interface IGmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null);
    Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string htmlBody, string? plainTextBody = null);
    Task<NotificationSendResult> SendEmailWithResultAsync(string to, string subject, string htmlBody, string? plainTextBody = null);
    Task<bool> IsServiceAvailableAsync();
}
