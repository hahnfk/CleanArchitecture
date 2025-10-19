namespace CleanArchitecture.Domain.Identity;

/// <summary>
/// Strongly-typed identity for the Task aggregate.
/// Using a record struct makes it lightweight, immutable,
/// and gives you value semantics and IEquatable support.
/// </summary>
public readonly record struct TodoId(Guid Value)
{
    /// <summary>
    /// Generate a new identity (guid-based). Use factories to centralize creation if needed.
    /// </summary>
    public static TodoId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}
