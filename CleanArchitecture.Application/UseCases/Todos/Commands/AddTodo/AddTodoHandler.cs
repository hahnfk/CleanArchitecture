using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.Abstractions.ROP;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;

namespace CleanArchitecture.Application.UseCases.Todos.Commands.AddTodo;

/// <summary>
/// Use case application service: orchestrates entities via repositories and UoW.
/// Contains no persistence code and no UI concerns.
/// </summary>
public sealed class AddTodoHandler : IUseCase<AddTodoRequest, AddTodoResponse>
{
    private readonly ITodoRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IDomainEventPublisher _publisher;

    public AddTodoHandler(ITodoRepository repo, IUnitOfWork uow, IDomainEventPublisher publisher)
    {
        _repo = repo;
        _uow = uow;
        _publisher = publisher;
    }

    public async Task<Result<AddTodoResponse>> Handle(AddTodoRequest request, CancellationToken ct = default)
    {
        // Basic input validation (Application layer)
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Fail<AddTodoResponse>(Error.Validation("Title must not be empty."));

        try
        {
            // Create aggregate (domain enforces its own invariants)
            var todo = new TodoItem(TodoId.New(), request.Title);

            await _repo.AddAsync(todo, ct);
            await _uow.SaveChangesAsync(ct);

            // Publish domain events after commit
            var events = todo.DomainEvents.ToArray();
            if (events.Length > 0)
                await _publisher.PublishAsync(events, ct);
            todo.ClearEvents();

            var response = new AddTodoResponse { Id = todo.Id.ToString(), Title = todo.Title };
            return Result<AddTodoResponse>.Ok(response);
        }
        catch (DuplicateTodoTitleException ex)
        {
            // Example: map domain-specific exception to a typed error
            return Result.Fail<AddTodoResponse>(Error.Conflict("A todo with the same title already exists.", ex.Message));
        }
        catch (Exception ex)
        {
            // Defensive last resort
            return Result.Fail<AddTodoResponse>(Error.Unexpected("Unexpected error while adding todo.", ex.Message));
        }
    }
}
