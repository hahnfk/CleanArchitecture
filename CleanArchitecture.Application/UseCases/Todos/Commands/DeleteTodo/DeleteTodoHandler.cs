namespace CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.Abstractions.ROP;
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

    public async Task<Result<Unit>> Handle(DeleteTodoRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.TodoId))
            return Result.Fail<Unit>(Error.Validation("TodoId must not be empty."));
        if (!Guid.TryParse(r.TodoId, out var g))
            return Result.Fail<Unit>(Error.Validation("TodoId is not a valid GUID."));

        try
        {
            // Option A: delete blindly (idempotent)
            var deleted = await _repo.DeleteAsync(new TodoId(g), ct);

            // If your repository returns a flag, you can map NotFound:
            // if (!deleted) return Result.Fail<Unit>(Error.NotFound($"Todo '{r.TodoId}' not found."));

            await _uow.SaveChangesAsync(ct);
            return Result<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result.Fail<Unit>(Error.Unexpected("Unexpected error while deleting todo.", ex.Message));
        }
    }
}
