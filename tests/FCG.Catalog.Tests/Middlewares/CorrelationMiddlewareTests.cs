using FCG.Catalog.Api.Middlewares;
using FCG.Catalog.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FCG.Catalog.Tests.Api.Middlewares;

public class CorrelationMiddlewareTests
{
    [Fact(DisplayName = "Validando criação de correlation id quando não enviado no header")]
    [Trait("Categoria", "API - Middlewares")]
    public async Task CorrelationMiddleware_CreateCorrelationId_WhenHeaderNotExists()
    {
        var correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();

        var context = new DefaultHttpContext();

        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask);

        await middleware.Invoke(context, correlationIdAccessorMock.Object);

        Assert.True(context.Response.Headers.ContainsKey("x-correlation-id"));

        correlationIdAccessorMock.Verify(
            x => x.Set(It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact(DisplayName = "Validando uso de correlation id enviado no header")]
    [Trait("Categoria", "API - Middlewares")]
    public async Task CorrelationMiddleware_UseCorrelationId_FromHeader()
    {
        var correlationId = Guid.NewGuid();

        var correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();

        var context = new DefaultHttpContext();

        context.Request.Headers["x-correlation-id"] = correlationId.ToString();

        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask);

        await middleware.Invoke(context, correlationIdAccessorMock.Object);

        Assert.Equal(correlationId.ToString(), context.Response.Headers["x-correlation-id"]);

        correlationIdAccessorMock.Verify(
            x => x.Set(correlationId),
            Times.Once);
    }
}