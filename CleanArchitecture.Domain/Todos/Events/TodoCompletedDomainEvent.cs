namespace CleanArchitecture.Domain.Todos.Events;

using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Identity;

/// <summary>
/// Domain event indicating that a task was completed.
/// Consumers (Application/Infrastructure) can react after commit,
/// e.g., send notifications, update projections, etc.
/// </summary>
public sealed class TodoCompletedDomainEvent : IDomainEvent
{
    public TodoCompletedDomainEvent(TodoId todoId) => TodoId = todoId;
    public TodoId TodoId { get; }
}
