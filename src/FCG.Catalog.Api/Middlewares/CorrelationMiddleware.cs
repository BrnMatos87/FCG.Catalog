using FCG.Catalog.Application.Contracts;

namespace FCG.Catalog.Api.Middlewares;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    private const string CorrelationIdHeader = "x-correlation-id";

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(
        HttpContext context,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        correlationIdAccessor.Set(correlationId);

        context.Request.Headers[CorrelationIdHeader] = correlationId.ToString();
        context.Response.Headers[CorrelationIdHeader] = correlationId.ToString();

        await _next(context);
    }

    private static Guid GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var value) &&
            Guid.TryParse(value, out var correlationId))
        {
            return correlationId;
        }

        return Guid.NewGuid();
    }
}