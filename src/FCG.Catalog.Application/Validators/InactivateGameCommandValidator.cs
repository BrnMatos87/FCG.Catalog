using FCG.Catalog.Application.Commands.Games;
using FluentValidation;

namespace FCG.Catalog.Application.Validators;

public class InactivateGameCommandValidator : AbstractValidator<InactivateGameCommand>
{
    public InactivateGameCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O identificador do jogo é obrigatório.");
    }
}