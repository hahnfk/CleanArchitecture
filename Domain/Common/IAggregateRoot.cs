namespace CleanArchitecture.Domain.Common;

/// <summary>
/// Marker interface to identify aggregate roots (only aggregate roots get repositories).
/// Useful for policies, interceptors, and repository constraints.
/// </summary>
public interface IAggregateRoot { }
