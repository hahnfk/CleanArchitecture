using System.Collections.Immutable;

namespace CleanArchitecture.Application.UseCases.Todos.Queries.ListTodos;

/// <summary>
/// Response DTO for listing todos. Flattened data for presentation.
/// </summary>
public sealed class ListTodosResponse
{
    public ImmutableArray<TodoDto> Items { get; init; } = ImmutableArray<TodoDto>.Empty;

    /// <summary>DTO shape optimized for consumption by UI/API layers.</summary>
    public sealed record TodoDto(string Id, string Title, bool IsCompleted);
}
