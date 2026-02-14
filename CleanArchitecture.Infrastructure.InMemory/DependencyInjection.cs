namespace CleanArchitecture.Infrastructure.InMemory;

using CleanArchitecture.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers infrastructure adapters (in-memory persistence, UoW).
/// This layer implements application ports. Swappable later (EF/DB, REST, etc.).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureInMemory(this IServiceCollection services)
    {
        // Persistence ports
        services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
        services.AddSingleton<IUnitOfWork, NoOpUnitOfWork>();

        return services;
    }
}
