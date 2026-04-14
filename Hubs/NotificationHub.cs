using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Smart_Notification_System.Hubs
{
    /// <summary>
    /// Real-time SignalR hub for pushing live notification events to connected clients.
    /// Connect via: ws://localhost:8080/hubs/notifications  (requires Bearer token)
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name ?? "anonymous";
            _logger.LogInformation("Client connected to NotificationHub: {ConnectionId} ({Username})",
                Context.ConnectionId, username);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected from NotificationHub: {ConnectionId}",
                Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
