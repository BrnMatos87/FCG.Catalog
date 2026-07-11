using FCG.Catalog.Application.Commands.Games;
using FCG.Catalog.Application.Validators;

namespace FCG.Catalog.Tests.Application.Validators;

public class UpdateGameCommandValidatorTests
{
    private readonly UpdateGameCommandValidator _validator;

    public UpdateGameCommandValidatorTests()
    {
        _validator = new UpdateGameCommandValidator();
    }

    [Fact(DisplayName = "Validando comando de atualização de jogo válido")]
    [Trait("Categoria", "Application - Validators")]
    public void UpdateGameCommandValidator_Valid()
    {
        var command = CreateValidCommand();

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validando identificador do jogo obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void UpdateGameCommandValidator_Id_Required()
    {
        var command = CreateValidCommand();
        command.Id = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador do jogo é obrigatório.");
    }

    [Fact(DisplayName = "Validando título obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void UpdateGameCommandValidator_Title_Required()
    {
        var command = CreateValidCommand();
        command.Title = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O título do jogo é obrigatório.");
    }

    [Fact(DisplayName = "Validando descrição obrigatória")]
    [Trait("Categoria", "Application - Validators")]
    public void UpdateGameCommandValidator_Description_Required()
    {
        var command = CreateValidCommand();
        command.Description = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "A descrição do jogo é obrigatória.");
    }

    [Fact(DisplayName = "Validando preço maior que zero")]
    [Trait("Categoria", "Application - Validators")]
    public void UpdateGameCommandValidator_Price_Invalid()
    {
        var command = CreateValidCommand();
        command.Price = 0;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O preço do jogo deve ser maior que zero.");
    }

    [Fact(DisplayName = "Validando categoria obrigatória")]
    [Trait("Categoria", "Application - Validators")]
    public void UpdateGameCommandValidator_Category_Required()
    {
        var command = CreateValidCommand();
        command.Category = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "A categoria do jogo é obrigatória.");
    }

    private static UpdateGameCommand CreateValidCommand()
    {
        return new UpdateGameCommand
        {
            Id = Guid.NewGuid(),
            Title = "Sonic",
            Description = "Jogo de aventura",
            Price = 199,
            Category = "Aventura"
        };
    }
}