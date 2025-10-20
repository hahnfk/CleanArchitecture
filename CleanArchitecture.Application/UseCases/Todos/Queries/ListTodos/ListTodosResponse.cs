namespace CleanArchitecture.Application.UseCases.Todos.Queries.ListTodos;

/// <summary>
/// Response DTO for listing todos. Flattened data for presentation.
/// </summary>
public sealed class ListTodosResponse
{
    public IReadOnlyList<TodoDto> Items { get; init; } = Array.Empty<TodoDto>();

    /// <summary>DTO shape optimized for consumption by UI/API layers.</summary>
    public sealed record TodoDto(string Id, string Title, bool IsCompleted);
}
