namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Base type for all domain entities.
/// Entities are compared by identity (Id), not by their field values.
/// Implements identity-based equality and hashing so sets/maps behave correctly.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    /// <summary>
    /// Globally unique identity inside the bounded context.
    /// Concrete entities decide what TId is (Guid-typed record, long, string, ...).
    /// </summary>
    public TId Id { get; protected init; }

    protected Entity(TId id)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        Id = id;
    }

    // Identity-based equality
    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    /// <summary>
    /// Two entities are equal if their Id is equal. Reference equality short-circuits.
    /// </summary>
    public bool Equals(Entity<TId>? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Hash code derived from Id to ensure dictionary/set stability.
    /// </summary>
    public override int GetHashCode()
    {
        // Ensure Id is not null before getting hash code
        if (Id is null)
            throw new InvalidOperationException("Entity Id cannot be null when computing hash code.");
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
