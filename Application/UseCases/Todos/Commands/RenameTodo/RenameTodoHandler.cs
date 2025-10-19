using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;

namespace CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;

/// <summary>
/// Use case: rename a Todo and publish the resulting domain event(s) after commit.
/// </summary>
public sealed class RenameTodoHandler
{
    private readonly ITodoRepository _todos;
    private readonly IUnitOfWork _uow;
    private readonly IDomainEventPublisher _publisher;

    public RenameTodoHandler(ITodoRepository Todos, IUnitOfWork uow, IDomainEventPublisher publisher)
    {
        _todos = Todos;
        _uow = uow;
        _publisher = publisher;
    }

    public async Task Handle(RenameTodoRequest r, CancellationToken ct = default)
    {
        if (!Guid.TryParse(r.TodoId, out var tg)) return;

        var Todo = await _todos.GetByIdAsync(new TodoId(tg), ct);
        if (Todo is null) return;

        Todo.Rename(r.NewTitle);

        await _todos.UpdateAsync(Todo, ct);
        await _uow.SaveChangesAsync(ct);

        // Publish domain events that were raised inside the aggregate
        await _publisher.PublishAsync(Todo.DomainEvents, ct);
        Todo.ClearEvents();
    }
}
