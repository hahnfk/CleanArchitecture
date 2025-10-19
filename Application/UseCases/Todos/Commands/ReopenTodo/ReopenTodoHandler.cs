namespace CleanArchitecture.Application.UseCases.Todos.Commands.ReopenTodo;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;
using System.Threading.Tasks;

public sealed class ReopenTodoHandler(ITodoRepository repo, IUnitOfWork uow)
{
    public async Task Handle(ReopenTodoRequest r, CancellationToken ct = default)
    {
        if (!Guid.TryParse(r.TodoId, out var g)) return;
        var todo = await repo.GetByIdAsync(new TodoId(g), ct);
        if (todo is null) return;

        // Domain should expose a method for this (idempotent)
        todo.Reopen();

        await repo.UpdateAsync(todo, ct);
        await uow.SaveChangesAsync(ct);
    }
}
