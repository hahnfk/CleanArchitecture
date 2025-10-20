namespace CleanArchitecture.Infrastructure.InMemory.Events;

using CleanArchitecture.Domain.Common;

/// <summary>
/// Infrastructure-side domain event handler contract. Implement one handler per event type.
/// Multiple handlers for the same event type are supported.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles a single domain event. Keep side effects idempotent and resilient.
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
