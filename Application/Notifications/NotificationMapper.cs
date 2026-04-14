using Smart_Notification_System.DTO;
using Smart_Notification_System.Models;

namespace Smart_Notification_System.Application.Notifications
{
    public static class NotificationMapper
    {
        public static NotificationResponseDto ToDto(Notification n) => new()
        {
            Id = n.Id,
            Message = n.Message,
            Type = n.Type,
            Priority = n.Priority,
            IsProcessed = n.IsProcessed,
            IsFailed = n.IsFailed,
            RetryCount = n.RetryCount,
            CreatedAt = n.CreatedAt,
            ProcessedAt = n.ProcessedAt,
            ScheduledAt = n.ScheduledAt
        };
    }
}
