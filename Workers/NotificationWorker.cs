using Microsoft.AspNetCore.SignalR;
using Smart_Notification_System.Application.Notifications;
using Smart_Notification_System.Hubs;
using Smart_Notification_System.Models;
using Smart_Notification_System.Repositories;

namespace Smart_Notification_System.Workers
{
    public class NotificationWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<NotificationWorker> _logger;
        private readonly IHubContext<NotificationHub, INotificationClient> _hub;
        private readonly int _intervalSeconds;
        private readonly int _maxRetryCount;

        public NotificationWorker(
            IServiceProvider provider,
            ILogger<NotificationWorker> logger,
            IConfiguration config,
            IHubContext<NotificationHub, INotificationClient> hub)
        {
            _provider = provider;
            _logger = logger;
            _hub = hub;
            _intervalSeconds = config.GetValue<int>("NotificationWorker:IntervalSeconds", 5);
            _maxRetryCount = config.GetValue<int>("NotificationWorker:MaxRetryCount", 3);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "NotificationWorker started. Interval={Interval}s, MaxRetry={MaxRetry}",
                _intervalSeconds, _maxRetryCount);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in NotificationWorker loop.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
            }

            _logger.LogInformation("NotificationWorker stopped.");
        }

        private async Task ProcessPendingNotificationsAsync()
        {
            using var scope = _provider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var pending = await repo.GetPendingAsync(_maxRetryCount);
            if (pending.Count == 0) return;

            _logger.LogInformation("Processing {Count} pending notification(s).", pending.Count);

            foreach (var notification in pending)
            {
                try
                {
                    await DeliverNotificationAsync(notification);

                    notification.IsProcessed = true;
                    notification.ProcessedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Notification processed: {@Notification}",
                        new { notification.Id, notification.Type, notification.Priority });

                    // Push real-time update to all connected SignalR clients
                    await _hub.Clients.All.NotificationProcessed(
                        NotificationMapper.ToDto(notification));
                }
                catch (Exception ex)
                {
                    notification.RetryCount++;

                    _logger.LogWarning(ex,
                        "Failed to process notification Id={Id}. Retry {Retry}/{Max}.",
                        notification.Id, notification.RetryCount, _maxRetryCount);

                    if (notification.RetryCount >= _maxRetryCount)
                    {
                        notification.IsFailed = true;
                        _logger.LogError(
                            "Notification Id={Id} permanently failed after {Max} retries.",
                            notification.Id, _maxRetryCount);

                        await _hub.Clients.All.NotificationFailed(
                            notification.Id, "Max retries exceeded.");
                    }
                }

                await repo.UpdateAsync(notification);
            }
        }

        private static Task DeliverNotificationAsync(Notification notification)
        {
            // TODO: Switch on notification.Type and call the appropriate channel:
            //   Email  → IEmailService.SendAsync(...)
            //   SMS    → ISmsService.SendAsync(...)
            //   Push   → IPushService.SendAsync(...)
            //   Webhook → IWebhookService.PostAsync(...)
            return Task.CompletedTask;
        }
    }
}
