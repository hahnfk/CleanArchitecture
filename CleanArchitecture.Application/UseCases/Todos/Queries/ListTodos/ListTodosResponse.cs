using System.Collections.Immutable;

namespace CleanArchitecture.Application.UseCases.Todos.Queries.ListTodos;

/// <summary>
/// Response DTO for listing todos. Flattened data for presentation.
/// </summary>
public sealed class ListTodosResponse
{
    public ImmutableArray<TodoDto> Items { get; init; } = ImmutableArray<TodoDto>.Empty;

    /// <summary>DTO shape optimized for consumption by UI/API layers.</summary>
    public sealed record TodoDto
    {
        public string Id { get; }
        public string Title { get; }
        public bool IsCompleted { get; }

        public TodoDto(string id, string title, bool isCompleted)
            => (Id, Title, IsCompleted) = (id, title, isCompleted);
    }
}
