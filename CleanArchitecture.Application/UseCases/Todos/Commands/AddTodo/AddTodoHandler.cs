namespace CleanArchitecture.Application.UseCases.Todos.Commands.AddTodo;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;

/// <summary>
/// Use case application service: orchestrates entities via repositories and UoW.
/// Contains no persistence code and no UI concerns.
/// </summary>
public sealed class AddTodoHandler : IUseCase<AddTodoRequest, AddTodoResponse>
{
    private readonly ITodoRepository _repo;
    private readonly IUnitOfWork _uow;

    public AddTodoHandler(ITodoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<AddTodoResponse> Handle(AddTodoRequest request, CancellationToken ct = default)
    {
        // Create a new aggregate and enforce invariants inside the domain model
        var todo = new TodoItem(TodoId.New(), request.Title);

        // Stage the changes (no commit yet)
        await _repo.AddAsync(todo, ct);

        // Commit transactional boundary
        await _uow.SaveChangesAsync(ct);

        return new AddTodoResponse { Id = todo.Id.ToString(), Title = todo.Title };
    }
}
