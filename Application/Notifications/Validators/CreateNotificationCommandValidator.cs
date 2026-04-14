using FluentValidation;
using Smart_Notification_System.Application.Notifications.Commands;

namespace Smart_Notification_System.Application.Notifications.Validators
{
    public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
    {
        private static readonly string[] ValidTypes = { "Email", "SMS", "Push", "Webhook" };
        private static readonly string[] ValidPriorities = { "Low", "Normal", "High", "Critical" };

        public CreateNotificationCommandValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required.")
                .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required.")
                .Must(t => ValidTypes.Contains(t))
                .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}.");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Priority is required.")
                .Must(p => ValidPriorities.Contains(p))
                .WithMessage($"Priority must be one of: {string.Join(", ", ValidPriorities)}.");

            RuleFor(x => x.ScheduledAt)
                .GreaterThan(DateTime.UtcNow).WithMessage("ScheduledAt must be a future date.")
                .When(x => x.ScheduledAt.HasValue);
        }
    }
}
