namespace CleanArchitecture.Infrastructure.Composition.DomainEvents;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Todos.Events;
using CleanArchitecture.Infrastructure.Composition.DomainEvents.Handlers;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the in-process domain event pipeline (publisher + handlers).
/// This must be provider-agnostic so switching persistence providers (InMemory, EF, ...) does not break domain events.
/// </summary>
public static class DomainEventsServiceCollectionExtensions
{
    public static IServiceCollection AddDomainEvents(this IServiceCollection services)
    {
        // Publisher
        services.AddSingleton<IDomainEventPublisher, ReflectionDomainEventPublisher>();

        // Example handlers (side effects)
        services.AddTransient<IDomainEventHandler<TodoCompletedDomainEvent>, TodoCompletedHandler>();
        services.AddTransient<IDomainEventHandler<TodoRenamedDomainEvent>, TodoRenamedHandler>();

        return services;
    }
}
