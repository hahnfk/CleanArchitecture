namespace CleanArchitecture.Contracts.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public PersistenceProvider Provider { get; set; } = PersistenceProvider.InMemory;

    /// <summary>
    /// Connection string for database providers that require it.
    /// For InMemory it can stay empty.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
