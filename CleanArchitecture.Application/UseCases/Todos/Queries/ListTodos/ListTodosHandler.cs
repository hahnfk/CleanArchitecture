using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.Abstractions.ROP;
using System.Collections.Immutable;

namespace CleanArchitecture.Application.UseCases.Todos.Queries.ListTodos;

/// <summary>
/// Use case: list all todos. In real-world code you'd add filtering, paging, sorting or use query objects.
/// </summary>
public sealed class ListTodosHandler : IUseCase<Unit, ListTodosResponse>
{
    private readonly ITodoRepository _repo;

    public ListTodosHandler(ITodoRepository repo) => _repo = repo;

    public async Task<Result<ListTodosResponse>> Handle(Unit unit, CancellationToken ct = default)
    {
        try
        {
            var all = await _repo.ListAsync(ct);
            var items = all
                .Select(t => new ListTodosResponse.TodoDto(t.Id.ToString(), t.Title, t.IsCompleted))
                .ToImmutableArray();

            return Result<ListTodosResponse>.Ok(new ListTodosResponse { Items = items });
        }
        catch (Exception ex)
        {
            return Result.Fail<ListTodosResponse>(Error.Unexpected("Failed to list todos.", ex.Message));
        }
    }
}
