using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;

namespace CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;

public sealed class DeleteTodoHandler(ITodoRepository repo, IUnitOfWork uow)
{
    public async Task Handle(DeleteTodoRequest r, CancellationToken ct = default)
    {
        if (!Guid.TryParse(r.TodoId, out var g)) return;
        await repo.DeleteAsync(new TodoId(g), ct);
        await uow.SaveChangesAsync(ct);
    }
}
