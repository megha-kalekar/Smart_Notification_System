namespace Smart_Notification_System.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public bool IsProcessed { get; set; }
        public bool IsFailed { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
