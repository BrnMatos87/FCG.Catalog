using FCG.Catalog.Api.Correlation;

namespace FCG.Catalog.Tests.Api.Correlation;

public class CorrelationIdAccessorTests
{
    [Fact(DisplayName = "Validando definição e recuperação do correlation id")]
    [Trait("Categoria", "API - Correlation")]
    public void CorrelationIdAccessor_Set_And_Get()
    {
        var correlationId = Guid.NewGuid();

        var accessor = new CorrelationIdAccessor();

        accessor.Set(correlationId);

        var result = accessor.Get();

        Assert.Equal(correlationId, result);
    }

    [Fact(DisplayName = "Validando correlation id padrão")]
    [Trait("Categoria", "API - Correlation")]
    public void CorrelationIdAccessor_Default()
    {
        var accessor = new CorrelationIdAccessor();

        var result = accessor.Get();

        Assert.Equal(Guid.Empty, result);
    }
}