namespace CleanArchitecture.Application.Abstractions;

/// <summary>
/// Transaction boundary abstraction. Application orchestrates work and requests a commit.
/// Infrastructure will join a DB transaction (or no-op for InMemory).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Commits the current unit of work. Returns number of affected entities where meaningful.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
