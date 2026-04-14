using Smart_Notification_System.Models;

namespace Smart_Notification_System.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification n);
        Task<Notification?> GetByIdAsync(int id);
        Task<(List<Notification> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? type, string? priority, bool? isProcessed);
        Task<List<Notification>> GetPendingAsync(int maxRetryCount);
        Task UpdateAsync(Notification n);
        Task SoftDeleteAsync(int id);
    }
}
