using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Application.Commands.Games;

namespace FCG.Catalog.Api.Mappers;

public static class GameMapper
{
    public static CreateGameCommand ToCommand(this CreateGameRequest request)
    {
        return new CreateGameCommand
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category
        };
    }

    public static UpdateGameCommand ToCommand(this UpdateGameRequest request, Guid id)
    {
        return new UpdateGameCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category
        };
    }
}