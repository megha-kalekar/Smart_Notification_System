using MediatR;
using Smart_Notification_System.DTO;
using Smart_Notification_System.Models;
using Smart_Notification_System.Repositories;

namespace Smart_Notification_System.Application.Notifications.Commands
{
    public class CreateNotificationCommandHandler
        : IRequestHandler<CreateNotificationCommand, NotificationResponseDto>
    {
        private readonly INotificationRepository _repo;

        public CreateNotificationCommandHandler(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<NotificationResponseDto> Handle(
            CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                Message = request.Message,
                Type = request.Type,
                Priority = request.Priority,
                ScheduledAt = request.ScheduledAt,
                IsProcessed = false
            };

            await _repo.AddAsync(notification);
            return NotificationMapper.ToDto(notification);
        }
    }
}
