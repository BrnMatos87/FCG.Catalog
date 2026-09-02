using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Application.Reviews.Contracts;
using FCG.Catalog.Application.Reviews.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.Api.Controllers;

[ApiController]
[Route("api/v1/games/{gameId:guid}/reviews")]
[Authorize]
public class GameReviewsController : ControllerBase
{
    private readonly IGameReviewRepository _repository;

    public GameReviewsController(
        IGameReviewRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid gameId,
        CreateGameReviewRequest request,
        CancellationToken ct)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(
                "Rating deve estar entre 1 e 5.");
        }

        var review = new GameReview
        {
            Id = Guid.NewGuid().ToString(),
            GameId = gameId,
            UserId = request.UserId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(review, ct);

        return Created(
            $"/api/v1/games/{gameId}/reviews/{review.Id}",
            review);
    }

    [HttpGet]
    public async Task<IActionResult> GetByGameIdAsync(
        Guid gameId,
        CancellationToken ct)
    {
        var reviews =
            await _repository.GetByGameIdAsync(gameId, ct);

        return Ok(reviews);
    }
}