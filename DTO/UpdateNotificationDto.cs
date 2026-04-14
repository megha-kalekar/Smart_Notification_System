using System.ComponentModel.DataAnnotations;

namespace Smart_Notification_System.DTO
{
    public class UpdateNotificationDto
    {
        [MaxLength(1000)]
        public string? Message { get; set; }

        [MaxLength(100)]
        public string? Type { get; set; }

        [RegularExpression("^(Low|Normal|High|Critical)$",
            ErrorMessage = "Priority must be Low, Normal, High, or Critical.")]
        public string? Priority { get; set; }

        public DateTime? ScheduledAt { get; set; }
    }
}
