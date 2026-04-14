namespace Smart_Notification_System.DTO
{
    public class NotificationRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public DateTime? ScheduledAt { get; set; }
    }
}
