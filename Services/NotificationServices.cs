using Smart_Notification_System.DTO;
using Smart_Notification_System.Models;
using Smart_Notification_System.Repositories;

namespace Smart_Notification_System.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<NotificationResponseDto> CreateAsync(NotificationRequestDto dto)
        {
            var n = new Notification
            {
                Message = dto.Message,
                Type = dto.Type,
                Priority = dto.Priority,
                ScheduledAt = dto.ScheduledAt,
                IsProcessed = false
            };

            await _repo.AddAsync(n);
            return MapToDto(n);
        }

        public async Task<PagedResponseDto<NotificationResponseDto>> GetAllAsync(
            int page, int pageSize, string? type, string? priority, bool? isProcessed)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (items, totalCount) = await _repo.GetPagedAsync(page, pageSize, type, priority, isProcessed);

            return new PagedResponseDto<NotificationResponseDto>
            {
                Data = items.Select(MapToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<NotificationResponseDto?> GetByIdAsync(int id)
        {
            var n = await _repo.GetByIdAsync(id);
            return n == null ? null : MapToDto(n);
        }

        public async Task<NotificationResponseDto?> UpdateAsync(int id, UpdateNotificationDto dto)
        {
            var n = await _repo.GetByIdAsync(id);
            if (n == null) return null;

            if (dto.Message != null) n.Message = dto.Message;
            if (dto.Type != null) n.Type = dto.Type;
            if (dto.Priority != null) n.Priority = dto.Priority;
            if (dto.ScheduledAt.HasValue) n.ScheduledAt = dto.ScheduledAt;

            await _repo.UpdateAsync(n);
            return MapToDto(n);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var n = await _repo.GetByIdAsync(id);
            if (n == null) return false;

            await _repo.SoftDeleteAsync(id);
            return true;
        }

        private static NotificationResponseDto MapToDto(Notification n) => new()
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
