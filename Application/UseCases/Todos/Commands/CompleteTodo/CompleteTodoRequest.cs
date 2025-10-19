namespace CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTask;

/// <summary>
/// Request DTO to complete a task by id.
/// </summary>
public sealed class CompleteTodoRequest
{
    public string Id { get; init; } = "";
}
