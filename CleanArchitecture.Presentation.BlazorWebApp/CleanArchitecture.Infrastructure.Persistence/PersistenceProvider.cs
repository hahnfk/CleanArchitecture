namespace CleanArchitecture.Infrastructure.Persistence;

/// <summary>
/// Logical persistence providers supported by this solution.
/// Add more providers (PostgreSQL, Oracle, MySql, MsSql, etc.) here.
/// </summary>
public enum PersistenceProvider
{
    InMemory = 0,
    EfSqlite = 1,
}
