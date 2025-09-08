using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Infrastructure.Services;

public class FirebaseService : IFirebaseService
{
    public Task<List<NotificationSendResult>> SendMultiCastAsync(NotificationRequest message, List<RecipientInfo> recipients)
    {
        throw new NotImplementedException();
    }

    public Task<NotificationSendResult> SendNotificationAsync(NotificationRequest message, RecipientInfo recipient)
    {
        throw new NotImplementedException();
    }
}