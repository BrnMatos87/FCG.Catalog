using FCG.Catalog.Api.Middlewares;
using FCG.Catalog.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Catalog.Tests.Api.Middlewares;

public class RequestLoggingMiddlewareTests
{
    [Fact(DisplayName = "Validando execução do middleware de logging")]
    [Trait("Categoria", "API - Middlewares")]
    public async Task RequestLoggingMiddleware_Invoke_Success()
    {
        var correlationId = Guid.NewGuid();

        var correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();

        correlationIdAccessorMock
            .Setup(x => x.Get())
            .Returns(correlationId);

        var loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();

        var wasCalled = false;

        var context = new DefaultHttpContext();

        var middleware = new RequestLoggingMiddleware(
            _ =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            },
            loggerMock.Object);

        await middleware.Invoke(context, correlationIdAccessorMock.Object);

        Assert.True(wasCalled);
    }
}