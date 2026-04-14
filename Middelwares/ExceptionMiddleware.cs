using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Smart_Notification_System
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await WriteProblemDetailsAsync(context, ex);
            }
        }

        private static async Task WriteProblemDetailsAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/problem+json";

            object response;

            if (ex is ValidationException validationEx)
            {
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

                var errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());

                response = new
                {
                    status = 422,
                    title = "Validation Failed",
                    detail = "One or more validation errors occurred.",
                    instance = context.Request.Path.Value,
                    traceId = context.TraceIdentifier,
                    errors
                };
            }
            else
            {
                var (statusCode, title) = ex switch
                {
                    UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Forbidden"),
                    KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
                    ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
                    InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid Operation"),
                    _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
                };

                context.Response.StatusCode = (int)statusCode;

                response = new ProblemDetails
                {
                    Status = (int)statusCode,
                    Title = title,
                    Detail = ex.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["traceId"] = context.TraceIdentifier }
                };
            }

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
