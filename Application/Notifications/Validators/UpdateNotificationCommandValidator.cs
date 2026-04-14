using FluentValidation;
using Smart_Notification_System.Application.Notifications.Commands;

namespace Smart_Notification_System.Application.Notifications.Validators
{
    public class UpdateNotificationCommandValidator : AbstractValidator<UpdateNotificationCommand>
    {
        private static readonly string[] ValidTypes = { "Email", "SMS", "Push", "Webhook" };
        private static readonly string[] ValidPriorities = { "Low", "Normal", "High", "Critical" };

        public UpdateNotificationCommandValidator()
        {
            RuleFor(x => x.Message)
                .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters.")
                .When(x => x.Message != null);

            RuleFor(x => x.Type)
                .Must(t => ValidTypes.Contains(t!))
                .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}.")
                .When(x => x.Type != null);

            RuleFor(x => x.Priority)
                .Must(p => ValidPriorities.Contains(p!))
                .WithMessage($"Priority must be one of: {string.Join(", ", ValidPriorities)}.")
                .When(x => x.Priority != null);

            RuleFor(x => x.ScheduledAt)
                .GreaterThan(DateTime.UtcNow).WithMessage("ScheduledAt must be a future date.")
                .When(x => x.ScheduledAt.HasValue);
        }
    }
}
