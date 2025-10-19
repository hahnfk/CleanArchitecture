namespace CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;

/// <summary>
/// Request to rename a Todo.
/// </summary>
public sealed class RenameTodoRequest
{
    public string TodoId { get; init; } = "";
    public string NewTitle { get; init; } = "";
}
