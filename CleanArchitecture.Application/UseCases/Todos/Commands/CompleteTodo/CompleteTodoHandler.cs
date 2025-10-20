namespace CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTodo;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;

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

    public async Task<Unit> Handle(CompleteTodoRequest request, CancellationToken ct = default)
    {
        // Parse incoming ID (UI/API shaped).
        if (!Guid.TryParse(request.Id, out var gid)) return Unit.Value;

        // Load aggregate via repository port.
        var todo = await _repo.GetByIdAsync(new TodoId(gid), ct);
        if (todo is null) return Unit.Value;

        // Apply domain behavior (emits event + bumps version).
        todo.Complete();

        // Persist changes and commit first (atomic state change).
        await _repo.UpdateAsync(todo, ct);
        await _uow.SaveChangesAsync(ct);

        // Publish domain events only AFTER a successful commit.
        var events = todo.DomainEvents.ToArray();
        await _publisher.PublishAsync(events, ct);

        // Clear events to avoid duplicate publication on re-entry.
        todo.ClearEvents();

        return Unit.Value;
    }
}
