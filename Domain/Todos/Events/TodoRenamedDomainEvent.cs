namespace CleanArchitecture.Domain.Todos.Events;

using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Identity;

/// <summary>
/// Domain event emitted when a Todo's title changes.
/// Carries both the old and new title to enable consumers (projections, notifications)
/// to react without reloading the aggregate.
/// </summary>
public sealed class TodoRenamedDomainEvent : IDomainEvent
{
    public TodoRenamedDomainEvent(TodoId todoId, string oldTitle, string newTitle)
    {
        TodoId = todoId;
        OldTitle = oldTitle;
        NewTitle = newTitle;
    }

    public TodoId TodoId { get; }
    public string OldTitle { get; }
    public string NewTitle { get; }
}
