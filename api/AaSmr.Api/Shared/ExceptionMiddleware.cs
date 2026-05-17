using System.Text.Json;
using System.Text.Json.Serialization;

namespace AaSmr.Api.Shared;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteAsync(context, 400, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(context, 404, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteAsync(context, 409, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await WriteAsync(context, 500, "An unexpected error occurred.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string error)
    {
        if (context.Response.HasStarted) return;
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(
            new ApiResponse<object>(false, Error: error), JsonOptions);
        await context.Response.WriteAsync(body);
    }
}
