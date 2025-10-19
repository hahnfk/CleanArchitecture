namespace CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTodo;

/// <summary>
/// Request DTO to complete a todo by id.
/// </summary>
public sealed class CompleteTodoRequest
{
    public string Id { get; init; } = "";
}
