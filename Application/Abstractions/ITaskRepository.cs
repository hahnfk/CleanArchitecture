namespace CleanArchitecture.Application.Abstractions;

using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;

/// <summary>
/// Output port for persistence: the Application layer depends on this interface,
/// not on any concrete DB. Infrastructure will implement this.
/// </summary>
public interface ITodoRepository
{
    /// <summary>Adds a new aggregate instance (no save/commit implied).</summary>
    Task AddAsync(TodoItem task, CancellationToken ct = default);

    /// <summary>Adds multiple aggregates in one logical operation (optional optimization).</summary>
    Task AddRangeAsync(IEnumerable<TodoItem> tasks, CancellationToken ct = default);

    /// <summary>Loads an aggregate by id or returns null if it doesn't exist.</summary>
    Task<TodoItem?> GetByIdAsync(TodoId id, CancellationToken ct = default);

    /// <summary>Returns all aggregates. In real apps use paging/filtering specifications.</summary>
    Task<IReadOnlyList<TodoItem>> ListAsync(CancellationToken ct = default);

    /// <summary>Persists changes to an existing aggregate instance.</summary>
    Task UpdateAsync(TodoItem task, CancellationToken ct = default);

    /// <summary>Deletes an aggregate by id. Ensure invariants (e.g., children) before calling.</summary>
    Task DeleteAsync(TodoId id, CancellationToken ct = default);
}
