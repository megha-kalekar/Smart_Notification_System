using Smart_Notification_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Smart_Notification_System.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;

        public NotificationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Notification n)
        {
            await _db.Notifications.AddAsync(n);
            await _db.SaveChangesAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
        }

        public async Task<(List<Notification> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? type, string? priority, bool? isProcessed)
        {
            var query = _db.Notifications.Where(n => !n.IsDeleted);

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(n => n.Type == type);

            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(n => n.Priority == priority);

            if (isProcessed.HasValue)
                query = query.Where(n => n.IsProcessed == isProcessed.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Notification>> GetPendingAsync(int maxRetryCount)
        {
            return await _db.Notifications
                .Where(n => !n.IsProcessed && !n.IsFailed && !n.IsDeleted
                            && n.RetryCount < maxRetryCount
                            && (n.ScheduledAt == null || n.ScheduledAt <= DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task UpdateAsync(Notification n)
        {
            _db.Notifications.Update(n);
            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n != null)
            {
                n.IsDeleted = true;
                await _db.SaveChangesAsync();
            }
        }
    }
}
