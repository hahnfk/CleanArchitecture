namespace CleanArchitecture.Application.UseCases.Tasks.Commands.AddTask;

/// <summary>
/// Response DTO for the AddTask use case.
/// </summary>
public sealed class AddTodoResponse
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
}
