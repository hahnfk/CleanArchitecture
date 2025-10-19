using CleanArchitecture.Application.UseCases.Tasks.Commands.AddTask;
using CleanArchitecture.Application.UseCases.Todos.Commands;
using CleanArchitecture.Application.UseCases.Todos.Commands.DeleteTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;
using CleanArchitecture.Application.UseCases.Todos.Commands.ReopenTodo;
using CleanArchitecture.Application.UseCases.Todos.Queries.ListTasks;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Application;

/// <summary>
/// Registers application services (use cases, pipelines, validators).
/// No infrastructure or UI dependencies.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Use cases (handlers)
        services.AddScoped<AddTodoHandler>();
        services.AddScoped<ListTodosHandler>();
        services.AddScoped<RenameTodoHandler>();
        services.AddScoped<DeleteTodoHandler>();
        services.AddScoped<CompleteTodoHandler>();
        services.AddScoped<ReopenTodoHandler>();

        return services;
    }
}
