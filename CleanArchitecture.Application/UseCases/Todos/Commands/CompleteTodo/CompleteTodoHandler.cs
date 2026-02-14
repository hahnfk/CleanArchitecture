using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.Abstractions.ROP;
using CleanArchitecture.Domain.Identity;

namespace CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTodo;

/// <summary>
/// Use case: mark a todo as completed (idempotent).
/// Emits a domain event within the aggregate; after commit, events are published.
/// </summary>

public sealed class CompleteTodoHandler : IUseCase<CompleteTodoRequest, Unit>
{
    private readonly ITodoRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IDomainEventPublisher _publisher;

    public CompleteTodoHandler(ITodoRepository repo, IUnitOfWork uow, IDomainEventPublisher publisher)
    {
        _repo = repo;
        _uow = uow;
        _publisher = publisher;
    }

    public async Task<Result<Unit>> Handle(CompleteTodoRequest request, CancellationToken ct = default)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.Id))
            return Result.Fail<Unit>(Error.Validation("Id must not be empty."));

        if (!Guid.TryParse(request.Id, out var gid))
            return Result.Fail<Unit>(Error.Validation("Id is not a valid GUID."));

        try
        {
            // Load aggregate
            var todo = await _repo.GetByIdAsync(new TodoId(gid), ct);
            if (todo is null)
                return Result.Fail<Unit>(Error.NotFound($"Todo '{request.Id}' not found."));

            // Apply domain behavior
            todo.Complete();

            // Persist + commit
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
            return Result.Fail<Unit>(Error.Unexpected("Unexpected error while completing todo.", ex.Message));
        }
    }
}
