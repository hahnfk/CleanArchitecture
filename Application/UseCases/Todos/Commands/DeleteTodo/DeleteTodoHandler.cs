using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;

namespace CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;

public sealed class DeleteTodoHandler(ITodoRepository repo, IUnitOfWork uow) : IUseCase<DeleteTodoRequest, Unit>
{
    public async Task<Unit> Handle(DeleteTodoRequest r, CancellationToken ct = default)
    {
        if (!Guid.TryParse(r.TodoId, out var g)) return Unit.Value;
        await repo.DeleteAsync(new TodoId(g), ct);
        await uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
