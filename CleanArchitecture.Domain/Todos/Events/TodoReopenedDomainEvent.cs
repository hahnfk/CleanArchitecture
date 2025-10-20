namespace CleanArchitecture.Domain.Todos.Events;

using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Identity;

/// <summary>
/// Domain event emitted when a completed todo item is reopened.
/// Consumers may listen to trigger follow-up actions such as
/// notifying collaborators or updating projections.
/// </summary>
public sealed class TodoReopenedDomainEvent : IDomainEvent
{
    public TodoReopenedDomainEvent(TodoId todoId) => TodoId = todoId;

    public TodoId TodoId { get; }
}
