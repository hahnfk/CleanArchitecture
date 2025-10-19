using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;

namespace CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;

/// <summary>
/// Use case: rename a Todo and publish the resulting domain event(s) after commit.
/// </summary>
public sealed class RenameTodoHandler
{
    private readonly ITodoRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IDomainEventPublisher _publisher;

    public RenameTodoHandler(ITodoRepository todoRepository, IUnitOfWork uow, IDomainEventPublisher publisher)
    {
        _repo = todoRepository;
        _uow = uow;
        _publisher = publisher;
    }

    public async Task Handle(RenameTodoRequest r, CancellationToken ct = default)
    {
        if (!Guid.TryParse(r.TodoId, out var gid)) return;

        var todo = await _repo.GetByIdAsync(new TodoId(gid), ct);
        if (todo is null) return;

        todo.Rename(r.NewTitle);

        await _repo.UpdateAsync(todo, ct);
        await _uow.SaveChangesAsync(ct);

        // Snapshot + publish only if there are events
        var events = todo.DomainEvents.ToArray();
        if (events.Length > 0)
            await _publisher.PublishAsync(events, ct);

        todo.ClearEvents();
    }
}
