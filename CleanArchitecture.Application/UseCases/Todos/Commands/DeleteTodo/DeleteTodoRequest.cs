namespace CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;

/// <summary>Use case: delete a todo by id.</summary>
public sealed class DeleteTodoRequest { public string TodoId { get; init; } = ""; }
