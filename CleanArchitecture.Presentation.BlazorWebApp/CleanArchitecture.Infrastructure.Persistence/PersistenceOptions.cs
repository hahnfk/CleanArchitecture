namespace CleanArchitecture.Infrastructure.Persistence;

/// <summary>
/// Configuration for persistence selection. This lives in Infrastructure only.
/// Presentation reads config and calls AddInfrastructure(...).
/// </summary>
public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public PersistenceProvider Provider { get; init; } = PersistenceProvider.InMemory;

    /// <summary>
    /// Connection string for DB-backed providers.
    /// For InMemory this is ignored.
    /// </summary>
    public string? ConnectionString { get; init; }
}
