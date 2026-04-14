using System.ComponentModel.DataAnnotations;

namespace Smart_Notification_System.DTO
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
