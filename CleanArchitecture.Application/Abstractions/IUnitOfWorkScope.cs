using CleanArchitecture.Application.Abstractions.ROP;

namespace CleanArchitecture.Application.Abstractions;

/// <summary>
/// Scoped transaction boundary. Wraps an action in begin/commit/rollback.
/// Rollback only occurs on exception or <see cref="Result{T}.IsFailure"/>.
/// </summary>
public interface IUnitOfWorkScope
{
    Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken ct = default);

    Task<Result<TResult>> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<Result<TResult>>> action,
        CancellationToken ct = default);
}