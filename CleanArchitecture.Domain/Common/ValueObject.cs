namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Base for class-based value objects if you don't want to use 'record'/'record struct'.
/// Provides structural equality via 'GetEqualityComponents'.
/// Prefer 'record' for brevity unless you need custom behavior.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Return the ordered sequence of components that define value equality.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType()) return false;

        using var a = GetEqualityComponents().GetEnumerator();
        using var b = other.GetEqualityComponents().GetEnumerator();

        while (a.MoveNext() && b.MoveNext())
            if (!Equals(a.Current, b.Current)) return false;

        return !a.MoveNext() && !b.MoveNext();
    }

    public override int GetHashCode()
        => GetEqualityComponents().Aggregate(0, (hash, obj) => HashCode.Combine(hash, obj));

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);
}
