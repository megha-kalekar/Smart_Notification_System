using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart_Notification_System.Application.Notifications.Commands;
using Smart_Notification_System.Application.Notifications.Queries;
using Smart_Notification_System.DTO;

namespace Smart_Notification_System.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Create a new notification. (Admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create([FromBody] NotificationRequestDto dto)
        {
            var command = new CreateNotificationCommand(dto.Message, dto.Type, dto.Priority, dto.ScheduledAt);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Get paginated notifications with optional filters.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseDto<NotificationResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? type = null,
            [FromQuery] string? priority = null,
            [FromQuery] bool? isProcessed = null)
        {
            var query = new GetPagedNotificationsQuery(page, pageSize, type, priority, isProcessed);
            return Ok(await _mediator.Send(query));
        }

        /// <summary>Get a single notification by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetNotificationByIdQuery(id));
            if (result == null)
                return NotFound(new { message = $"Notification {id} not found." });
            return Ok(result);
        }

        /// <summary>Partially update a notification. (Admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}")]
        [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNotificationDto dto)
        {
            var command = new UpdateNotificationCommand(id, dto.Message, dto.Type, dto.Priority, dto.ScheduledAt);
            var result = await _mediator.Send(command);
            if (result == null)
                return NotFound(new { message = $"Notification {id} not found." });
            return Ok(result);
        }

        /// <summary>Soft-delete a notification. (Admin only)</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _mediator.Send(new DeleteNotificationCommand(id));
            if (!deleted)
                return NotFound(new { message = $"Notification {id} not found." });
            return NoContent();
        }
    }
}
