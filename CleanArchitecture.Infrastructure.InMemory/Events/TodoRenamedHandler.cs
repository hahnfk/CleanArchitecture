using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Todos.Events;

namespace CleanArchitecture.Infrastructure.InMemory.Events;

/// <summary>
/// Example handler for TodoRenamedDomainEvent.
/// Replace Debug.WriteLine with your real side-effects (update read model, send email, etc.).
/// </summary>
internal sealed class TodoRenamedHandler : IDomainEventHandler<TodoRenamedDomainEvent>
{
    public Task HandleAsync(TodoRenamedDomainEvent e, CancellationToken ct = default)
    {
        System.Diagnostics.Debug.WriteLine($"[EVENT] Todo renamed: {e.TodoId} \"{e.OldTitle}\" -> \"{e.NewTitle}\"");
        return Task.CompletedTask;
    }
}
