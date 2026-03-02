namespace CleanArchitecture.Infrastructure.Ado.Sqlite;

/// <summary>
/// Thrown when a handwritten ADO.NET UPDATE or DELETE affects zero rows
/// because the concurrency token (Version) did not match.
/// Analogous to <c>DbUpdateConcurrencyException</c> in EF Core.
/// </summary>
public sealed class DbConcurrencyException : Exception
{
    public DbConcurrencyException(string message) : base(message) { }
    public DbConcurrencyException(string message, Exception inner) : base(message, inner) { }
}
