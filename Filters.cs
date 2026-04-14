using Microsoft.AspNetCore.Mvc.Filters;

namespace Smart_Notification_System
{
    public class LoggingFilter : IActionFilter
    {
        private readonly ILogger<LoggingFilter> _logger;

        public LoggingFilter(ILogger<LoggingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.ActionDescriptor.DisplayName;
            var user = context.HttpContext.User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("Executing action: {Action} by user: {User}", action, user);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var action = context.ActionDescriptor.DisplayName;
            var statusCode = context.HttpContext.Response.StatusCode;
            _logger.LogInformation("Completed action: {Action} with status: {StatusCode}", action, statusCode);
        }
    }
}
