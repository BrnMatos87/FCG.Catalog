using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Api.Mappers;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Queries.Games;
using FCG.Catalog.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(
        [FromBody] CreateGameRequest request,
        [FromServices] ICommandHandler<CreateGameCommand, Guid> handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(request.ToCommand(), ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            new { id });
    }

    [HttpGet]
    [Authorize(Roles = "User,Administrator")]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<GetGamesQuery, IList<GameResponse>> handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(new GetGamesQuery(), ct);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "User,Administrator")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetGameByIdQuery, GameResponse?> handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(new GetGameByIdQuery
        {
            Id = id
        }, ct);

        if (response is null)
            return NotFound(new { message = "Jogo não encontrado." });

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateGameRequest request,
        [FromServices] ICommandHandlerVoid<UpdateGameCommand> handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(request.ToCommand(id), ct);

        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id,
        [FromServices] ICommandHandlerVoid<ActivateGameCommand> handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ActivateGameCommand
        {
            Id = id
        }, ct);

        return NoContent();
    }

    [HttpPatch("{id:guid}/inactivate")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Inactivate(
        [FromRoute] Guid id,
        [FromServices] ICommandHandlerVoid<InactivateGameCommand> handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new InactivateGameCommand
        {
            Id = id
        }, ct);

        return NoContent();
    }
}