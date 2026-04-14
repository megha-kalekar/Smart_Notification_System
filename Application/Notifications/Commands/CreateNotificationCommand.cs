using MediatR;
using Smart_Notification_System.DTO;

namespace Smart_Notification_System.Application.Notifications.Commands
{
    public record CreateNotificationCommand(
        string Message,
        string Type,
        string Priority,
        DateTime? ScheduledAt
    ) : IRequest<NotificationResponseDto>;
}
