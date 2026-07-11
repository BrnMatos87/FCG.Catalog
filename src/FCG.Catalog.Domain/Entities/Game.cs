using FCG.BuildingBlocks.Domain;
using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Domain.Exceptions;

namespace FCG.Catalog.Domain.Entities;

public class Game : EntityBase
{
    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public string Category { get; private set; } = string.Empty;

    protected Game()
    {
    }

    public static Game Create(
        string title,
        string description,
        decimal price,
        string category)
    {
        var normalizedTitle = title?.Trim() ?? string.Empty;
        var normalizedDescription = description?.Trim() ?? string.Empty;
        var normalizedCategory = category?.Trim() ?? string.Empty;

        ValidateTitle(normalizedTitle);
        ValidateDescription(normalizedDescription);
        ValidatePrice(price);
        ValidateCategory(normalizedCategory);

        return new Game
        {
            Id = Guid.NewGuid(),
            Title = normalizedTitle,
            Description = normalizedDescription,
            Price = price,
            Category = normalizedCategory,
            Status = StatusType.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string title,
        string description,
        decimal price,
        string category)
    {
        EnsureActive();

        var normalizedTitle = title?.Trim() ?? string.Empty;
        var normalizedDescription = description?.Trim() ?? string.Empty;
        var normalizedCategory = category?.Trim() ?? string.Empty;

        ValidateTitle(normalizedTitle);
        ValidateDescription(normalizedDescription);
        ValidatePrice(price);
        ValidateCategory(normalizedCategory);

        Title = normalizedTitle;
        Description = normalizedDescription;
        Price = price;
        Category = normalizedCategory;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("O título do jogo é obrigatório.");

        if (title.Length > 150)
            throw new DomainException("O título do jogo deve ter no máximo 150 caracteres.");
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("A descrição do jogo é obrigatória.");

        if (description.Length > 500)
            throw new DomainException("A descrição do jogo deve ter no máximo 500 caracteres.");
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("O preço do jogo deve ser maior que zero.");
    }

    private static void ValidateCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("A categoria do jogo é obrigatória.");

        if (category.Length > 100)
            throw new DomainException("A categoria do jogo deve ter no máximo 100 caracteres.");
    }

    private void EnsureActive()
    {
        if (Status != StatusType.Active)
            throw new DomainException("O jogo está inativo.");
    }
}