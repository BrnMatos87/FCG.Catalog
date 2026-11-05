namespace FCG.Catalog.Infrastructure.Search;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string IndexName { get; set; } = "fcg-games";
}
