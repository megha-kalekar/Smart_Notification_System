using MediatR;
using Smart_Notification_System.DTO;

namespace Smart_Notification_System.Application.Notifications.Queries
{
    public record GetPagedNotificationsQuery(
        int Page,
        int PageSize,
        string? Type,
        string? Priority,
        bool? IsProcessed
    ) : IRequest<PagedResponseDto<NotificationResponseDto>>;
}
