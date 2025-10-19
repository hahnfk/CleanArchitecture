
using CleanArchitecture.Application.Tasks;
using CleanArchitecture.Application.UseCases.Tasks.Commands.AddTask;
using CleanArchitecture.Application.UseCases.Tasks.Queries.ListTasks;
using CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;
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
        services.AddScoped<CompleteTodoHandler>();
        services.AddScoped<RenameTodoHandler>();

        return services;
    }
}
