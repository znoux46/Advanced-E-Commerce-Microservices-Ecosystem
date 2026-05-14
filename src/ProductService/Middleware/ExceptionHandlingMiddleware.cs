using System.Net;
using System.Text.Json;

namespace ProductService.Middleware;

/// <summary>
/// Custom middleware for handling exceptions globally
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = context.Response;
        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case ArgumentException argumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse
                {
                    StatusCode = response.StatusCode,
                    Message = argumentException.Message,
                    Details = "Bad request"
                };
                break;
                
            case KeyNotFoundException keyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new ErrorResponse
                {
                    StatusCode = response.StatusCode,
                    Message = keyNotFoundException.Message,
                    Details = "Resource not found"
                };
                break;
                
            case UnauthorizedAccessException unauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = new ErrorResponse
                {
                    StatusCode = response.StatusCode,
                    Message = unauthorizedAccessException.Message,
                    Details = "Unauthorized access"
                };
                break;
                
            case InvalidOperationException invalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse
                {
                    StatusCode = response.StatusCode,
                    Message = invalidOperationException.Message,
                    Details = "Invalid operation"
                };
                break;
                
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new ErrorResponse
                {
                    StatusCode = response.StatusCode,
                    Message = "An internal error occurred",
                    Details = exception.Message
                };
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(result);
    }
}

/// <summary>
/// Error response model
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
