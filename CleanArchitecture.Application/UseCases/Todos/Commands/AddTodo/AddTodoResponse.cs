namespace CleanArchitecture.Application.UseCases.Todos.Commands.AddTodo;

/// <summary>
/// Response DTO for the AddTodo use case.
/// </summary>
public sealed class AddTodoResponse
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
}
