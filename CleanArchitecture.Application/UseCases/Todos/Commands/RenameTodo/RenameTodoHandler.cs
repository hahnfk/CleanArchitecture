using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.Abstractions.ROP;
using CleanArchitecture.Domain.Identity;

namespace CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;

/// <summary>
/// Use case: rename a todo and publish the resulting domain event(s) after commit.
/// </summary>
public sealed class RenameTodoHandler : IUseCase<RenameTodoRequest, Unit>
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

    public async Task<Result<Unit>> Handle(RenameTodoRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.TodoId))
            return Result.Fail<Unit>(Error.Validation("TodoId must not be empty."));
        if (!Guid.TryParse(r.TodoId, out var gid))
            return Result.Fail<Unit>(Error.Validation("TodoId is not a valid GUID."));
        if (string.IsNullOrWhiteSpace(r.NewTitle))
            return Result.Fail<Unit>(Error.Validation("NewTitle must not be empty."));

        try
        {
            var todo = await _repo.GetByIdAsync(new TodoId(gid), ct);
            if (todo is null)
                return Result.Fail<Unit>(Error.NotFound($"Todo '{r.TodoId}' not found."));

            todo.Rename(r.NewTitle);

            await _repo.UpdateAsync(todo, ct);
            await _uow.SaveChangesAsync(ct);

            var events = todo.DomainEvents.ToArray();
            if (events.Length > 0)
                await _publisher.PublishAsync(events, ct);
            todo.ClearEvents();

            return Result<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result.Fail<Unit>(Error.Unexpected("Unexpected error while renaming todo.", ex.Message));
        }
    }
}
