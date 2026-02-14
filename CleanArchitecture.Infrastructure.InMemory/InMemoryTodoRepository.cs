namespace CleanArchitecture.Infrastructure.InMemory;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using System.Collections.Concurrent;

/// <summary>
/// Thread-safe in-memory repository implementing the application's output port.
/// Ideal for tests, demos, and as a starting stub before swapping to a DB.
/// </summary>
internal sealed class InMemoryTodoRepository : ITodoRepository
{
    // Acts as our fake database table keyed by aggregate id
    private readonly ConcurrentDictionary<Guid, TodoItem> _store = new();

    public Task AddAsync(TodoItem todo, CancellationToken ct = default)
    {
        _store [todo.Id.Value] = todo; // Upsert semantics are fine for the demo
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<TodoItem> todos, CancellationToken ct = default)
    {
        foreach (var todo in todos) _store [todo.Id.Value] = todo;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(TodoId id, CancellationToken ct = default)
    {
        var deleted = _store.TryRemove(id.Value, out _);
        return Task.FromResult(deleted);
    }

    public Task<TodoItem?> GetByIdAsync(TodoId id, CancellationToken ct = default)
    {
        _store.TryGetValue(id.Value, out var entity);
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<TodoItem>> ListAsync(CancellationToken ct = default)
    {
        // keep deterministic ordering for UI expectations
        var list = _store.Values.OrderBy(v => v.Title).ToList();
        return Task.FromResult((IReadOnlyList<TodoItem>)list);
    }

    public Task UpdateAsync(TodoItem todo, CancellationToken ct = default)
    {
        _store [todo.Id.Value] = todo;
        return Task.CompletedTask;
    }
}
