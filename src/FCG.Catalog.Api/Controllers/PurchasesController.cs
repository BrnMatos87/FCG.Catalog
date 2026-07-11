using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Api.Mappers;
using FCG.Catalog.Application.Abstractions.Commands;
using FCG.Catalog.Application.Abstractions.Queries;
using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Application.Queries.Library;
using FCG.Catalog.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize(Roles = "User,Administrator")]
public sealed class PurchasesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseRequest request,
        [FromServices] ICommandHandler<CreatePurchaseCommand, Guid> handler,
        CancellationToken ct)
    {
        var orderId = await handler.HandleAsync(request.ToCommand(), ct);

        return Accepted(new
        {
            orderId,
            message = "Pedido de compra recebido. O pagamento será processado."
        });
    }

    [HttpGet("users/{userId:guid}/library")]
    public async Task<IActionResult> GetUserLibrary(
        [FromRoute] Guid userId,
        [FromServices] IQueryHandler<GetUserLibraryQuery, IList<GameLibraryResponse>> handler,
        CancellationToken ct)
    {
        var response = await handler.HandleAsync(new GetUserLibraryQuery
        {
            UserId = userId
        }, ct);

        return Ok(response);
    }
}