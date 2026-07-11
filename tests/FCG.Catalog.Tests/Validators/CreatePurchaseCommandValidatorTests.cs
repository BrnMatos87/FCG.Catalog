using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Application.Validators;

namespace FCG.Catalog.Tests.Application.Validators;

public class CreatePurchaseCommandValidatorTests
{
    private readonly CreatePurchaseCommandValidator _validator;

    public CreatePurchaseCommandValidatorTests()
    {
        _validator = new CreatePurchaseCommandValidator();
    }

    [Fact(DisplayName = "Validando comando de compra válido")]
    [Trait("Categoria", "Application - Validators")]
    public void CreatePurchaseCommandValidator_Valid()
    {
        var command = CreateValidCommand();

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validando identificador do usuário obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void CreatePurchaseCommandValidator_UserId_Required()
    {
        var command = CreateValidCommand();
        command.UserId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador do usuário é obrigatório.");
    }

    [Fact(DisplayName = "Validando identificador do jogo obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void CreatePurchaseCommandValidator_GameId_Required()
    {
        var command = CreateValidCommand();
        command.GameId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador do jogo é obrigatório.");
    }

    [Fact(DisplayName = "Validando e-mail do usuário obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void CreatePurchaseCommandValidator_UserEmail_Required()
    {
        var command = CreateValidCommand();
        command.UserEmail = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O e-mail do usuário é obrigatório.");
    }

    [Fact(DisplayName = "Validando formato de e-mail do usuário")]
    [Trait("Categoria", "Application - Validators")]
    public void CreatePurchaseCommandValidator_UserEmail_Invalid()
    {
        var command = CreateValidCommand();
        command.UserEmail = "email-invalido";

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O e-mail do usuário está em formato inválido.");
    }

    private static CreatePurchaseCommand CreateValidCommand()
    {
        return new CreatePurchaseCommand
        {
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com"
        };
    }
}