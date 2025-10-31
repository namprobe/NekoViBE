using NekoViBE.Application.Common.Enums;

namespace NekoViBE.Application.Common.Interfaces;

public interface INotificationFactory
{
    INotificationService GetSender(NotificationChannelEnum channel);
}