namespace CleanArchitecture.Application.UseCases.Todos.Commands.ReopenTodo;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;
using CleanArchitecture.Domain.Identity;
using System.Threading.Tasks;

public sealed class ReopenTodoHandler(ITodoRepository repo, IUnitOfWork uow) : IUseCase<ReopenTodoRequest, Unit>
{
    public async Task<Unit> Handle(ReopenTodoRequest r, CancellationToken ct = default)
    {
        if (!Guid.TryParse(r.TodoId, out var g)) return Unit.Value;
        var todo = await repo.GetByIdAsync(new TodoId(g), ct);
        if (todo is null) return Unit.Value;

        // Domain should expose a method for this (idempotent)
        todo.Reopen();

        await repo.UpdateAsync(todo, ct);
        await uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
