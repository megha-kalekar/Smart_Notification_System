using Smart_Notification_System.DTO;

namespace Smart_Notification_System.Hubs
{
    /// <summary>
    /// Typed SignalR client interface — avoids magic string method names.
    /// </summary>
    public interface INotificationClient
    {
        Task NotificationProcessed(NotificationResponseDto notification);
        Task NotificationFailed(int notificationId, string reason);
    }
}
