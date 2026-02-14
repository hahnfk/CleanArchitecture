namespace CleanArchitecture.Application.UseCases.Todos.Commands.ReopenTodo;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Application.Abstractions.ROP;

/// <summary>
/// Use case: reopen a todo that has been completed.
/// </summary>
public sealed class ReopenTodoHandler : IUseCase<ReopenTodoRequest, Unit>
{
    private readonly ITodoRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IDomainEventPublisher _publisher;

    public ReopenTodoHandler(ITodoRepository repo, IUnitOfWork uow, IDomainEventPublisher publisher)
    {
        _repo = repo;
        _uow = uow;
        _publisher = publisher;
    }

    public async Task<Result<Unit>> Handle(ReopenTodoRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.TodoId))
            return Result.Fail<Unit>(Error.Validation("TodoId must not be empty."));

        if (!Guid.TryParse(r.TodoId, out var g))
            return Result.Fail<Unit>(Error.Validation("TodoId is not a valid GUID."));

        try
        {
            var todo = await _repo.GetByIdAsync(new TodoId(g), ct);
            if (todo is null)
                return Result.Fail<Unit>(Error.NotFound($"Todo '{r.TodoId}' not found."));

            todo.Reopen();

            await _repo.UpdateAsync(todo, ct);
            await _uow.SaveChangesAsync(ct);

            // Publish domain events after commit
            var events = todo.DomainEvents.ToArray();
            if (events.Length > 0)
                await _publisher.PublishAsync(events, ct);
            todo.ClearEvents();

            return Result<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result.Fail<Unit>(Error.Unexpected("Unexpected error while reopening todo.", ex.Message));
        }
    }
}
