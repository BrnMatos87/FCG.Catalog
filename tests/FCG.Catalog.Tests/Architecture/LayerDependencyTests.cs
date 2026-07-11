using FCG.Catalog.Api.Controllers;
using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Infrastructure.Persistence;
using FCG.Catalog.Worker.Consumers;
using NetArchTest.Rules;

namespace FCG.Catalog.Tests.Architecture;

public class LayerDependencyTests
{
    [Fact(DisplayName = "Domain não deve depender de Application")]
    [Trait("Categoria", "Architecture")]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types
            .InAssembly(typeof(Game).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Application")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Domain não deve depender de Infrastructure")]
    [Trait("Categoria", "Architecture")]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(Game).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Domain não deve depender de Api")]
    [Trait("Categoria", "Architecture")]
    public void Domain_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(typeof(Game).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Domain não deve depender de Worker")]
    [Trait("Categoria", "Architecture")]
    public void Domain_Should_Not_Depend_On_Worker()
    {
        var result = Types
            .InAssembly(typeof(Game).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Application não deve depender de Infrastructure")]
    [Trait("Categoria", "Architecture")]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(CreateGameCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Application não deve depender de Api")]
    [Trait("Categoria", "Architecture")]
    public void Application_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(typeof(CreateGameCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Application não deve depender de Worker")]
    [Trait("Categoria", "Architecture")]
    public void Application_Should_Not_Depend_On_Worker()
    {
        var result = Types
            .InAssembly(typeof(CreateGameCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Infrastructure não deve depender de Api")]
    [Trait("Categoria", "Architecture")]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(typeof(CatalogDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Infrastructure não deve depender de Worker")]
    [Trait("Categoria", "Architecture")]
    public void Infrastructure_Should_Not_Depend_On_Worker()
    {
        var result = Types
            .InAssembly(typeof(CatalogDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Api não deve depender de Worker")]
    [Trait("Categoria", "Architecture")]
    public void Api_Should_Not_Depend_On_Worker()
    {
        var result = Types
            .InAssembly(typeof(GamesController).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact(DisplayName = "Worker não deve depender de Api")]
    [Trait("Categoria", "Architecture")]
    public void Worker_Should_Not_Depend_On_Api()
    {
        var result = Types
            .InAssembly(typeof(PaymentProcessedConsumer).Assembly)
            .ShouldNot()
            .HaveDependencyOn("FCG.Catalog.Api")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}