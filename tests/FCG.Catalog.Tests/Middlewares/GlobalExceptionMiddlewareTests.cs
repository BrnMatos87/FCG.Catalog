using System.Text.Json;
using FCG.Catalog.Api.Middlewares;
using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FCG.Catalog.Tests.Api.Middlewares;

public class GlobalExceptionMiddlewareTests
{
    private readonly Mock<ICorrelationIdAccessor> _correlationIdAccessorMock;
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _loggerMock;

    public GlobalExceptionMiddlewareTests()
    {
        _correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();
        _loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
    }

    [Fact(DisplayName = "Validando tratamento de DomainException")]
    [Trait("Categoria", "API - Middlewares")]
    public async Task GlobalExceptionMiddleware_HandleDomainException()
    {
        var correlationId = Guid.NewGuid();

        _correlationIdAccessorMock
            .Setup(x => x.Get())
            .Returns(correlationId);

        var context = CreateHttpContext();

        var middleware = new GlobalExceptionMiddleware(
            _ => throw new DomainException("Erro de domínio."),
            _loggerMock.Object);

        await middleware.Invoke(context, _correlationIdAccessorMock.Object);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var body = await ReadResponseBodyAsync(context);

        Assert.Contains("Erro de domínio.", body);
        Assert.Contains(correlationId.ToString(), body);
    }

    [Fact(DisplayName = "Validando tratamento de InvalidOperationException")]
    [Trait("Categoria", "API - Middlewares")]
    public async Task GlobalExceptionMiddleware_HandleInvalidOperationException()
    {
        var correlationId = Guid.NewGuid();

        _correlationIdAccessorMock
            .Setup(x => x.Get())
            .Returns(correlationId);

        var context = CreateHttpContext();

        var middleware = new GlobalExceptionMiddleware(
            _ => throw new InvalidOperationException("Operação inválida."),
            _loggerMock.Object);

        await middleware.Invoke(context, _correlationIdAccessorMock.Object);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var body = await ReadResponseBodyAsync(context);

        Assert.Contains("Operação inválida.", body);
        Assert.Contains(correlationId.ToString(), body);
    }

    [Fact(DisplayName = "Validando tratamento de UnauthorizedAccessException")]
    [Trait("Categoria", "API - Middlewares")]
    public async Task GlobalExceptionMiddleware_HandleUnauthorizedAccessException()
    {
        var correlationId = Guid.NewGuid();

        _correlationIdAccessorMock
            .Setup(x => x.Get())
            .Returns(correlationId);

        var context = CreateHttpContext();

        var middleware = new GlobalExceptionMiddleware(
            _ => throw new UnauthorizedAccessException("Não autorizado."),
            _loggerMock.Object);

        await middleware.Invoke(context, _correlationIdAccessorMock.Object);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);

        var body = await ReadResponseBodyAsync(context);

        Assert.Contains("Não autorizado.", body);
        Assert.Contains(correlationId.ToString(), body);
    }

    [Fact(DisplayName = "Validando tratamento de exceção inesperada")]
    [Trait("Categoria", "API - Middlewares")]
    public async Task GlobalExceptionMiddleware_HandleUnexpectedException()
    {
        var correlationId = Guid.NewGuid();

        _correlationIdAccessorMock
            .Setup(x => x.Get())
            .Returns(correlationId);

        var context = CreateHttpContext();

        var middleware = new GlobalExceptionMiddleware(
            _ => throw new Exception("Erro interno."),
            _loggerMock.Object);

        await middleware.Invoke(context, _correlationIdAccessorMock.Object);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var body = await ReadResponseBodyAsync(context);

        Assert.Contains("Erro inesperado.", body);
        Assert.Contains(correlationId.ToString(), body);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();

        context.Response.Body = new MemoryStream();

        return context;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(context.Response.Body);

        return await reader.ReadToEndAsync();
    }
}