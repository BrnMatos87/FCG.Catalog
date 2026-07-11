using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Commands.Purchases;
using FCG.Catalog.Application.Validators;

namespace FCG.Catalog.Tests.Application.Validators;

public class ProcessPaymentCommandValidatorTests
{
    private readonly ProcessPaymentCommandValidator _validator;

    public ProcessPaymentCommandValidatorTests()
    {
        _validator = new ProcessPaymentCommandValidator();
    }

    [Fact(DisplayName = "Validando comando de processamento de pagamento válido")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessPaymentCommandValidator_Valid()
    {
        var command = CreateValidCommand();

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validando identificador do pedido obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessPaymentCommandValidator_OrderId_Required()
    {
        var command = CreateValidCommand();
        command.OrderId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador do pedido é obrigatório.");
    }

    [Fact(DisplayName = "Validando identificador do usuário obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessPaymentCommandValidator_UserId_Required()
    {
        var command = CreateValidCommand();
        command.UserId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador do usuário é obrigatório.");
    }

    [Fact(DisplayName = "Validando identificador do jogo obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessPaymentCommandValidator_GameId_Required()
    {
        var command = CreateValidCommand();
        command.GameId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador do jogo é obrigatório.");
    }

    [Fact(DisplayName = "Validando e-mail do usuário obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessPaymentCommandValidator_UserEmail_Required()
    {
        var command = CreateValidCommand();
        command.UserEmail = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O e-mail do usuário é obrigatório.");
    }

    [Fact(DisplayName = "Validando título do jogo obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessPaymentCommandValidator_GameTitle_Required()
    {
        var command = CreateValidCommand();
        command.GameTitle = string.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O título do jogo é obrigatório.");
    }

    [Fact(DisplayName = "Validando preço da compra maior que zero")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessPaymentCommandValidator_Price_Invalid()
    {
        var command = CreateValidCommand();
        command.Price = 0;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O preço da compra deve ser maior que zero.");
    }

    [Fact(DisplayName = "Validando status de pagamento inválido")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessPaymentCommandValidator_Status_Invalid()
    {
        var command = CreateValidCommand();
        command.Status = (PaymentStatus)999;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O status do pagamento deve ser aprovado ou rejeitado.");
    }

    [Fact(DisplayName = "Validando identificador de correlação obrigatório")]
    [Trait("Categoria", "Application - Validators")]
    public void ProcessPaymentCommandValidator_CorrelationId_Required()
    {
        var command = CreateValidCommand();
        command.CorrelationId = Guid.Empty;

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "O identificador de correlação é obrigatório.");
    }

    private static ProcessPaymentCommand CreateValidCommand()
    {
        return new ProcessPaymentCommand
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            UserEmail = "usuario@email.com",
            GameTitle = "Sonic",
            Price = 199,
            Status = PaymentStatus.Approved,
            CorrelationId = Guid.NewGuid()
        };
    }
}