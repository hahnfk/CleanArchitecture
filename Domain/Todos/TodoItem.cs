namespace CleanArchitecture.Domain.Todos;

using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos.Events;
using CleanArchitecture.Domain.Todos.Events;

/// <summary>
/// Aggregate root representing a to-do task. Encapsulates invariants and behavior.
/// Note: No persistence or UI concerns here (Domain stays pure).
/// </summary>
public sealed class TodoItem : AggregateRoot<TodoId>
{
    private string _title = string.Empty;

    /// <summary>
    /// Aggregate constructor validates invariants and establishes a valid state.
    /// </summary>
    public TodoItem(TodoId id, string title) : base(id)
    {
        Rename(title);
    }

    /// <summary>
    /// Human-readable title of the task; cannot be empty.
    /// </summary>
    public string Title => _title;

    /// <summary>
    /// Indicates whether the task is done.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Business operation to rename a task with invariant checks.
    /// Emits TaskRenamedDomainEvent only if the title actually changes (idempotent).
    /// </summary>
    public void Rename(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Title must not be empty.", nameof(newTitle));

        var normalized = newTitle.Trim();

        // Idempotent: no event if nothing changes
        if (string.Equals(_title, normalized, StringComparison.Ordinal))
            return;

        var old = _title;
        _title = normalized;

        // Record the domain event for later publication (no infra dependency)
        RaiseEvent(new TodoRenamedDomainEvent(Id, old, _title));
        IncrementVersion();
    }

    /// <summary>
    /// Business operation to complete the task; idempotent by design.
    /// Emits a domain event and bumps version for persistence layers.
    /// </summary>
    public void Complete()
    {
        if (IsCompleted) return;
        IsCompleted = true;

        // Emit a domain event; no direct infrastructure calls here.
        RaiseEvent(new TodoCompletedDomainEvent(Id));

        // Optional: increment optimistic concurrency token for persistence.
        IncrementVersion();
    }
}
