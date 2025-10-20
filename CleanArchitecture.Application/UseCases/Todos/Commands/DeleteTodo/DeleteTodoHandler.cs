namespace CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;

/// <summary>
/// Use case: delete a todo aggregate.
/// </summary>
public sealed class DeleteTodoHandler : IUseCase<DeleteTodoRequest, Unit>
{
    private readonly ITodoRepository _repo;
    private readonly IUnitOfWork _uow;

    public DeleteTodoHandler(ITodoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Unit> Handle(DeleteTodoRequest r, CancellationToken ct = default)
    {
        if (!Guid.TryParse(r.TodoId, out var g)) return Unit.Value;

        await _repo.DeleteAsync(new TodoId(g), ct);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
