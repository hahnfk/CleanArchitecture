using CleanArchitecture.Application.Abstractions;

namespace CleanArchitecture.Infrastructure.InMemory;

/// <summary>
/// In-memory UoW: nothing to commit, but keeps the application contract intact.
/// Swapping to a DB-backed UoW later will require no changes in the application layer.
/// </summary>
internal sealed class NoOpUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
}
