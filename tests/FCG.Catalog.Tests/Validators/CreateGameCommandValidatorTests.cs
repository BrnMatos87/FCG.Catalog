using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Validators;

namespace FCG.Catalog.Tests.Application.Validators;

public class CreateGameCommandValidatorTests
{
    private readonly CreateGameCommandValidator _validator;

    public CreateGameCommandValidatorTests()
    {
        _validator = new CreateGameCommandValidator();
    }

    [Fact(DisplayName = "Validando comando de criação de jogo válido")]
    [Trait("Categoria", "Application - Validators")]
    public void CreateGameCommandValidator_Valid()
    {
        var command = new CreateGameCommand
        {
            Title = "Sonic",
            Description = "Jogo de aventura",
            Price = 199,
            Category = "Aventura"
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validando título obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void CreateGameCommandValidator_Title_Required()
    {
        var command = CreateValidCommand();
        command.Title = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O título do jogo é obrigatório.");
    }

    [Fact(DisplayName = "Validando descrição obrigatória")]
    [Trait("Categoria", "Application - Validators")]
    public void CreateGameCommandValidator_Description_Required()
    {
        var command = CreateValidCommand();
        command.Description = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "A descrição do jogo é obrigatória.");
    }

    [Fact(DisplayName = "Validando preço maior que zero")]
    [Trait("Categoria", "Application - Validators")]
    public void CreateGameCommandValidator_Price_Invalid()
    {
        var command = CreateValidCommand();
        command.Price = 0;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O preço do jogo deve ser maior que zero.");
    }

    [Fact(DisplayName = "Validando categoria obrigatória")]
    [Trait("Categoria", "Application - Validators")]
    public void CreateGameCommandValidator_Category_Required()
    {
        var command = CreateValidCommand();
        command.Category = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "A categoria do jogo é obrigatória.");
    }

    private static CreateGameCommand CreateValidCommand()
    {
        return new CreateGameCommand
        {
            Title = "Sonic",
            Description = "Jogo de aventura",
            Price = 199,
            Category = "Aventura"
        };
    }
}