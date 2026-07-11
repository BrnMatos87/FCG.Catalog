using FCG.Catalog.Application.Commands.Purchases;
using FluentValidation;

namespace FCG.Catalog.Application.Validators;

public class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("O identificador do usuário é obrigatório.");

        RuleFor(x => x.GameId)
            .NotEmpty()
            .WithMessage("O identificador do jogo é obrigatório.");

        RuleFor(x => x.UserEmail)
            .NotEmpty()
            .WithMessage("O e-mail do usuário é obrigatório.")
            .EmailAddress()
            .WithMessage("O e-mail do usuário está em formato inválido.");
    }
}