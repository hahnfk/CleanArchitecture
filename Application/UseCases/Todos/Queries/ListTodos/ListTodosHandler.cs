namespace CleanArchitecture.Application.UseCases.Todos.Queries.ListTasks;

using CleanArchitecture.Application.Abstractions;

/// <summary>
/// Use case: List all tasks. In real-world code you'd add filtering, paging, sorting or use query objects.
/// </summary>
public sealed class ListTodosHandler
{
    private readonly ITodoRepository _repo;

    public ListTodosHandler(ITodoRepository repo) => _repo = repo;

    public async Task<ListTodosResponse> Handle(CancellationToken ct = default)
    {
        var all = await _repo.ListAsync(ct);
        var items = all.Select(t => new ListTodosResponse.TodoDto(t.Id.ToString(), t.Title, t.IsCompleted)).ToList();
        return new ListTodosResponse { Items = items };
    }
}
