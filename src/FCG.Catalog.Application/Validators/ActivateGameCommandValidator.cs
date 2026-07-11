using FCG.Catalog.Application.Commands.Games;
using FluentValidation;

namespace FCG.Catalog.Application.Validators;

public class ActivateGameCommandValidator : AbstractValidator<ActivateGameCommand>
{
    public ActivateGameCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O identificador do jogo é obrigatório.");
    }
}