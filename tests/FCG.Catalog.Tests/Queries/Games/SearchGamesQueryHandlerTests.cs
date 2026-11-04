using FCG.Catalog.Application.Contracts;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Queries.Games.Handlers;
using FCG.Catalog.Application.Responses;
using Moq;

namespace FCG.Catalog.Tests.Application.Queries.Games;

public sealed class SearchGamesQueryHandlerTests
{
    private readonly Mock<IGameSearchService> _gameSearchServiceMock = new();

    [Fact(DisplayName = "Busca jogos usando termo e paginação")]
    [Trait("Categoria", "Application - SearchGames")]
    public async Task HandleAsync_ValidQuery_SearchesGames()
    {
        var expected = new List<GameSearchResult>
        {
            new() { Id = Guid.NewGuid(), Title = "Sonic", Relevance = 4.2 }
        };

        _gameSearchServiceMock
            .Setup(x => x.SearchAsync("sonic", 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new SearchGamesQueryHandler(_gameSearchServiceMock.Object);

        var result = await handler.HandleAsync(new SearchGamesQuery
        {
            Term = " sonic ",
            Page = 2,
            PageSize = 5
        });

        Assert.Equal(expected, result);
    }

    [Theory(DisplayName = "Rejeita parâmetros inválidos de busca")]
    [InlineData("", 1, 10)]
    [InlineData("sonic", 0, 10)]
    [InlineData("sonic", 1, 51)]
    [Trait("Categoria", "Application - SearchGames")]
    public async Task HandleAsync_InvalidQuery_Throws(
        string term,
        int page,
        int pageSize)
    {
        var handler = new SearchGamesQueryHandler(_gameSearchServiceMock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new SearchGamesQuery
        {
            Term = term,
            Page = page,
            PageSize = pageSize
        }));
    }
}
