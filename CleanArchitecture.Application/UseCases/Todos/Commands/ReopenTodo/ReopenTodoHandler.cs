namespace CleanArchitecture.Application.UseCases.Todos.Commands.ReopenTodo;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;

/// <summary>
/// Use case: reopen a todo that has been completed.
/// </summary>
public sealed class ReopenTodoHandler : IUseCase<ReopenTodoRequest, Unit>
{
    private readonly ITodoRepository _repo;
    private readonly IUnitOfWork _uow;

    public ReopenTodoHandler(ITodoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Unit> Handle(ReopenTodoRequest r, CancellationToken ct = default)
    {
        if (!Guid.TryParse(r.TodoId, out var g)) return Unit.Value;

        var todo = await _repo.GetByIdAsync(new TodoId(g), ct);
        if (todo is null) return Unit.Value;

        todo.Reopen();

        await _repo.UpdateAsync(todo, ct);
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
