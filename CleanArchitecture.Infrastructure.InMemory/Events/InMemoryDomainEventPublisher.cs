namespace CleanArchitecture.Infrastructure.InMemory.Events;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Common;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Simple in-memory domain event publisher:
/// Resolves all registered IDomainEventHandler<TEvent> for each event and invokes HandleAsync.
/// NOTE: HandleAsync itself is NOT generic; the interface is generic. Do NOT call MakeGenericMethod.
/// </summary>
internal sealed class InMemoryDomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _sp;

    public InMemoryDomainEventPublisher(IServiceProvider sp) => _sp = sp;

    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var e in events)
        {
            // Build the closed generic handler interface for the concrete event type
            var handlerInterface = typeof(IDomainEventHandler<>).MakeGenericType(e.GetType());

            // Resolve all handlers registered for this event type
            var handlers = _sp.GetServices(handlerInterface);
            if (handlers is null) continue;

            // Get the non-generic HandleAsync method from the closed interface
            var handleAsync = handlerInterface.GetMethod(
                nameof(IDomainEventHandler<IDomainEvent>.HandleAsync),
                new [] { e.GetType(), typeof(CancellationToken) }
            );

            if (handleAsync is null)
                throw new InvalidOperationException(
                    $"Could not find HandleAsync on {handlerInterface.FullName}."
                );

            // Invoke each handler; HandleAsync returns Task
            foreach (var handler in handlers)
            {
                var task = (Task?)handleAsync.Invoke(handler, new object [] { e, ct });
                if (task is not null)
                    await task.ConfigureAwait(false);
            }
        }
    }
}
