using FCG.Catalog.Api.DTOs.Requests;
using FCG.Catalog.Application.Commands.Purchases;

namespace FCG.Catalog.Api.Mappers;

public static class PurchaseMapper
{
    public static CreatePurchaseCommand ToCommand(this CreatePurchaseRequest request)
    {
        return new CreatePurchaseCommand
        {
            UserId = request.UserId,
            GameId = request.GameId,
            UserEmail = request.UserEmail
        };
    }
}