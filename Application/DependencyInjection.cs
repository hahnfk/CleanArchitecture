using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.UseCases.Tasks.Commands.AddTask;
using CleanArchitecture.Application.UseCases.Todos.Commands;
using CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTask;
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
        // Commands
        services.AddScoped<AddTodoHandler>();
        services.AddScoped<IUseCase<AddTodoRequest, AddTodoResponse>>(sp => sp.GetRequiredService<AddTodoHandler>());

        services.AddScoped<CompleteTodoHandler>();
        services.AddScoped<IUseCase<CompleteTodoRequest, Unit>>(sp => sp.GetRequiredService<CompleteTodoHandler>());

        services.AddScoped<RenameTodoHandler>();
        services.AddScoped<IUseCase<RenameTodoRequest, Unit>>(sp => sp.GetRequiredService<RenameTodoHandler>());

        services.AddScoped<ReopenTodoHandler>();
        services.AddScoped<IUseCase<ReopenTodoRequest, Unit>>(sp => sp.GetRequiredService<ReopenTodoHandler>());

        services.AddScoped<DeleteTodoHandler>();
        services.AddScoped<IUseCase<DeleteTodoRequest, Unit>>(sp => sp.GetRequiredService<DeleteTodoHandler>());

        // Queries
        services.AddScoped<ListTodosHandler>();
        services.AddScoped<IUseCase<Unit, ListTodosResponse>>(sp => sp.GetRequiredService<ListTodosHandler>());

        return services;
    }
}
