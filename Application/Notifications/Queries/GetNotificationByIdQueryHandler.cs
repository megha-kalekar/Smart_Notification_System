using MediatR;
using Smart_Notification_System.DTO;
using Smart_Notification_System.Repositories;

namespace Smart_Notification_System.Application.Notifications.Queries
{
    public class GetNotificationByIdQueryHandler
        : IRequestHandler<GetNotificationByIdQuery, NotificationResponseDto?>
    {
        private readonly INotificationRepository _repo;

        public GetNotificationByIdQueryHandler(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<NotificationResponseDto?> Handle(
            GetNotificationByIdQuery request, CancellationToken cancellationToken)
        {
            var notification = await _repo.GetByIdAsync(request.Id);
            return notification == null ? null : NotificationMapper.ToDto(notification);
        }
    }
}
