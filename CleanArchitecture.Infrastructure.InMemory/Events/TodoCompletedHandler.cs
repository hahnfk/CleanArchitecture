namespace CleanArchitecture.Infrastructure.InMemory.Events;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Todos.Events;

/// <summary>
/// Example event handler for TodoCompletedDomainEvent.
/// Replace Debug.WriteLine with your side effect (email, projection update, message enqueue, ...).
/// </summary>
internal sealed class TodoCompletedHandler : IDomainEventHandler<TodoCompletedDomainEvent>
{
    public Task HandleAsync(TodoCompletedDomainEvent e, CancellationToken ct = default)
    {
        System.Diagnostics.Debug.WriteLine($"[EVENT] Task completed: {e.TodoId}");
        return Task.CompletedTask;
    }
}
