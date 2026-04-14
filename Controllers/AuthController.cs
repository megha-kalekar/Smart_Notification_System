using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Notification_System.DTO;
using Smart_Notification_System.Models;
using Smart_Notification_System.Services;

namespace Smart_Notification_System.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext db, JwtService jwt, ILogger<AuthController> logger)
        {
            _db = db;
            _jwt = jwt;
            _logger = logger;
        }

        /// <summary>Register a new user account.</summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var exists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
            if (exists)
                return Conflict(new { message = $"Username '{dto.Username}' is already taken." });

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Username} with role {Role}", user.Username, user.Role);
            return CreatedAtAction(nameof(Register), new { message = "User registered successfully." });
        }

        /// <summary>Authenticate and receive access + refresh tokens.</summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", dto.Username);
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(await IssueTokensAsync(user));
        }

        /// <summary>Exchange a valid refresh token for a new access token.</summary>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var tokenHash = _jwt.HashRefreshToken(dto.RefreshToken);

            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.RefreshTokenHash == tokenHash &&
                u.RefreshTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                _logger.LogWarning("Invalid or expired refresh token used.");
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            // Rotate refresh token
            return Ok(await IssueTokensAsync(user));
        }

        /// <summary>Revoke the current refresh token (logout).</summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
        {
            var tokenHash = _jwt.HashRefreshToken(dto.RefreshToken);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == tokenHash);

            if (user != null)
            {
                user.RefreshTokenHash = null;
                user.RefreshTokenExpiry = null;
                await _db.SaveChangesAsync();
                _logger.LogInformation("User {Username} logged out.", user.Username);
            }

            return NoContent();
        }

        private async Task<object> IssueTokensAsync(User user)
        {
            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            user.RefreshTokenHash = _jwt.HashRefreshToken(refreshToken);
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
            await _db.SaveChangesAsync();

            return new
            {
                accessToken,
                refreshToken,
                expiresIn = _jwt.ExpiryMinutes * 60,
                role = user.Role
            };
        }
    }
}
