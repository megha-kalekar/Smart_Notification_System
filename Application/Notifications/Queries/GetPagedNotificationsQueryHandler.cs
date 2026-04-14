using MediatR;
using Smart_Notification_System.DTO;
using Smart_Notification_System.Repositories;

namespace Smart_Notification_System.Application.Notifications.Queries
{
    public class GetPagedNotificationsQueryHandler
        : IRequestHandler<GetPagedNotificationsQuery, PagedResponseDto<NotificationResponseDto>>
    {
        private readonly INotificationRepository _repo;

        public GetPagedNotificationsQueryHandler(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResponseDto<NotificationResponseDto>> Handle(
            GetPagedNotificationsQuery request, CancellationToken cancellationToken)
        {
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var (items, totalCount) = await _repo.GetPagedAsync(
                page, pageSize, request.Type, request.Priority, request.IsProcessed);

            return new PagedResponseDto<NotificationResponseDto>
            {
                Data = items.Select(NotificationMapper.ToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
