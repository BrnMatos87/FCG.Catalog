namespace FCG.Catalog.Application.Contracts;

public interface ICorrelationIdAccessor
{
    Guid Get();

    void Set(Guid correlationId);
}