namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Marker interface for all domain events emitted by aggregate roots.
/// Keep events in the domain model; do not depend on infrastructure.
/// </summary>
public interface IDomainEvent { }
