namespace CleanArchitecture.Application.Tasks;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.UseCases.Tasks.Commands.CompleteTask;
using CleanArchitecture.Domain.Identity;

/// <summary>
/// Use case: mark a task as completed (idempotent).
/// Emits a domain event within the aggregate; after commit, events are published.
/// </summary>
public sealed class CompleteTodoHandler
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

    public async Task Handle(CompleteTodoRequest request, CancellationToken ct = default)
    {
        // Parse incoming ID (UI/API shaped).
        if (!Guid.TryParse(request.Id, out var gid)) return;

        // Load aggregate via repository port.
        var task = await _repo.GetByIdAsync(new TodoId(gid), ct);
        if (task is null) return;

        // Apply domain behavior (emits event + bumps version).
        task.Complete();

        // Persist changes and commit first (atomic state change).
        await _repo.UpdateAsync(task, ct);
        await _uow.SaveChangesAsync(ct);

        // Publish domain events only AFTER a successful commit.
        await _publisher.PublishAsync(task.DomainEvents, ct);

        // Clear events to avoid duplicate publication on re-entry.
        task.ClearEvents();
    }
}
