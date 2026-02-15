namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Aggregate root base class: builds on Entity, adds domain event handling
/// and an optional optimistic concurrency Version.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>
    /// Collected domain events that occurred on this aggregate since last clear.
    /// Application/Infrastructure may dispatch them (e.g., Outbox).
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event for later publication (no direct infrastructure dependency).
    /// </summary>
    protected void RaiseEvent(IDomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _domainEvents.Add(@event);
    }

    /// <summary>
    /// Clears pending events (usually after successful transaction commit).
    /// </summary>
    public void ClearEvents() => _domainEvents.Clear();

    /// <summary>
    /// Optional optimistic concurrency token for persistence layers like EF Core.
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// The version as loaded from persistence.
    /// Infrastructure uses this as the EF Core concurrency-token original value.
    /// </summary>
    public long OriginalVersion { get; private set; }

    /// <summary>
    /// Call after state changes that should bump the concurrency version.
    /// </summary>
    public void IncrementVersion() => Version++;

    /// <summary>
    /// For repository rehydration only.
    /// </summary>
    protected internal void SetVersion(long version)
    {
        Version = version;
        OriginalVersion = version;
    }
}
