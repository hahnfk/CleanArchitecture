namespace CleanArchitecture.Infrastructure.EfCore.Sqlite.Db;

/// <summary>
/// Persistence model for EF Core.
/// We keep it separate from the Domain aggregate to avoid persistence concerns
/// in the Domain and to control rehydration (no domain events on load).
/// </summary>
internal sealed class TodoRow
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public long Version { get; set; }
}
