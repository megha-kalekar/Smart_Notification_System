using MediatR;
using Smart_Notification_System.DTO;

namespace Smart_Notification_System.Application.Notifications.Queries
{
    public record GetNotificationByIdQuery(int Id) : IRequest<NotificationResponseDto?>;
}
