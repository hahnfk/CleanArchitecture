namespace CleanArchitecture.Application.Abstractions;

using CleanArchitecture.Domain.Common;

/// <summary>
/// Application-level port to publish a batch of domain events after committing
/// the transaction. Infrastructure implements this port (e.g., in-memory,
/// MediatR, Outbox to a message broker, etc.).
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes the provided domain events. Must be idempotent and safe to call
    /// after the transaction is committed.
    /// </summary>
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
