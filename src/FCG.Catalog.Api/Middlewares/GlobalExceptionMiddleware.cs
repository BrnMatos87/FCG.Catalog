using System.Text.Encodings.Web;
using System.Text.Json;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Exceptions;

namespace FCG.Catalog.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status401Unauthorized, correlationIdAccessor);
        }
        catch (DomainException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest, correlationIdAccessor);
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest, correlationIdAccessor);
        }
        catch (Exception ex)
        {
            var correlationId = correlationIdAccessor.Get();

            _logger.LogError(
                ex,
                "Erro inesperado {Method} {Path} CorrelationId {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(new
            {
                message = "Erro inesperado.",
                correlationId
            }, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            await context.Response.WriteAsync(json);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception ex,
        int statusCode,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        var correlationId = correlationIdAccessor.Get();

        _logger.LogWarning(
            ex,
            "Erro tratado {Method} {Path} StatusCode {StatusCode} CorrelationId {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            statusCode,
            correlationId);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(new
        {
            message = ex.Message,
            correlationId
        }, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        await context.Response.WriteAsync(json);
    }
}