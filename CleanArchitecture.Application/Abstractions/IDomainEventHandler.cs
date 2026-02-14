using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Application.Abstractions;

/// <summary>
/// Application-layer contract for domain event handlers.
/// Implement one handler per event type. Multiple handlers for the same event type are supported.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles a single domain event. Keep side effects idempotent and resilient.
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
