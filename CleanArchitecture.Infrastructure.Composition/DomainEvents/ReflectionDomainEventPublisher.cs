namespace CleanArchitecture.Infrastructure.Composition.DomainEvents;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// In-process domain event publisher:
/// Resolves all registered <see cref="IDomainEventHandler{TEvent}"/> for each event and invokes HandleAsync.
///
/// Notes:
/// - This is intentionally persistence-agnostic.
/// - If you later introduce outbox, messaging, retries, etc. this can be replaced behind the same port.
/// </summary>
internal sealed class ReflectionDomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _sp;

    public ReflectionDomainEventPublisher(IServiceProvider sp) => _sp = sp;

    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var e in events)
        {
            var handlerInterface = typeof(IDomainEventHandler<>).MakeGenericType(e.GetType());

            var handlers = _sp.GetServices(handlerInterface);
            if (handlers is null)
                continue;

            var handleAsync = handlerInterface.GetMethod(
                nameof(IDomainEventHandler<IDomainEvent>.HandleAsync),
                new[] { e.GetType(), typeof(CancellationToken) }
            );

            if (handleAsync is null)
                throw new InvalidOperationException($"Could not find HandleAsync on {handlerInterface.FullName}.");

            foreach (var handler in handlers)
            {
                var task = (Task?)handleAsync.Invoke(handler, new object[] { e, ct });
                if (task is not null)
                    await task.ConfigureAwait(false);
            }
        }
    }
}
