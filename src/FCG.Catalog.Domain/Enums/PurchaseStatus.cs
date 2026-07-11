using System.ComponentModel;

namespace FCG.Catalog.Domain.Enums;

public enum PurchaseStatus
{
    [Description("Pendente")]
    Pending = 1,
    [Description("Aprovada")]
    Approved = 2,
    [Description("Rejeitada")]
    Rejected = 3
}