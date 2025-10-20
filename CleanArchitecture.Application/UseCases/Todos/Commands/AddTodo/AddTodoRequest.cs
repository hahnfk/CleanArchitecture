namespace CleanArchitecture.Application.UseCases.Todos.Commands.AddTodo;

/// <summary>
/// Request DTO for the AddTodo use case. Keep DTOs decoupled from domain types
/// unless it's a shared value object; they're API-shaped.
/// </summary>
public sealed class AddTodoRequest
{
    public string Title { get; init; } = "";
}


