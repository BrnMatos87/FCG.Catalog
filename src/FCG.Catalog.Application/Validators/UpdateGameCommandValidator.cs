using FCG.Catalog.Application.Commands.Games;
using FluentValidation;

namespace FCG.Catalog.Application.Validators;

public class UpdateGameCommandValidator : AbstractValidator<UpdateGameCommand>
{
    public UpdateGameCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O identificador do jogo é obrigatório.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("O título do jogo é obrigatório.")
            .MaximumLength(150)
            .WithMessage("O título do jogo deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("A descrição do jogo é obrigatória.")
            .MaximumLength(500)
            .WithMessage("A descrição do jogo deve ter no máximo 500 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("O preço do jogo deve ser maior que zero.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("A categoria do jogo é obrigatória.")
            .MaximumLength(100)
            .WithMessage("A categoria do jogo deve ter no máximo 100 caracteres.");
    }
}