using MediatR;
using Smart_Notification_System.DTO;
using Smart_Notification_System.Repositories;

namespace Smart_Notification_System.Application.Notifications.Commands
{
    public class UpdateNotificationCommandHandler
        : IRequestHandler<UpdateNotificationCommand, NotificationResponseDto?>
    {
        private readonly INotificationRepository _repo;

        public UpdateNotificationCommandHandler(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<NotificationResponseDto?> Handle(
            UpdateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _repo.GetByIdAsync(request.Id);
            if (notification == null) return null;

            if (request.Message != null) notification.Message = request.Message;
            if (request.Type != null) notification.Type = request.Type;
            if (request.Priority != null) notification.Priority = request.Priority;
            if (request.ScheduledAt.HasValue) notification.ScheduledAt = request.ScheduledAt;

            await _repo.UpdateAsync(notification);
            return NotificationMapper.ToDto(notification);
        }
    }
}
