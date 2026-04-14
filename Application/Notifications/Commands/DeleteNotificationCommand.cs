using MediatR;

namespace Smart_Notification_System.Application.Notifications.Commands
{
    public record DeleteNotificationCommand(int Id) : IRequest<bool>;
}
