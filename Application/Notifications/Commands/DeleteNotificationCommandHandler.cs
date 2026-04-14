using MediatR;
using Smart_Notification_System.Repositories;

namespace Smart_Notification_System.Application.Notifications.Commands
{
    public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
    {
        private readonly INotificationRepository _repo;

        public DeleteNotificationCommandHandler(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _repo.GetByIdAsync(request.Id);
            if (notification == null) return false;

            await _repo.SoftDeleteAsync(request.Id);
            return true;
        }
    }
}
