using FCG.BuildingBlocks.Enums;
using FCG.Catalog.Application.Commands.Purchases;
using FluentValidation;

namespace FCG.Catalog.Application.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("O identificador do pedido é obrigatório.");

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

        RuleFor(x => x.GameTitle)
            .NotEmpty()
            .WithMessage("O título do jogo é obrigatório.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("O preço da compra deve ser maior que zero.");

        RuleFor(x => x.Status)
            .Must(status => status == PaymentStatus.Approved || status == PaymentStatus.Rejected)
            .WithMessage("O status do pagamento deve ser aprovado ou rejeitado.");

        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .WithMessage("O identificador de correlação é obrigatório.");
    }
}