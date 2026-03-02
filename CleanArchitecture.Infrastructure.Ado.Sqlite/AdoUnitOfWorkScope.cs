using System.Data.Common;
using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.Abstractions.ROP;

namespace CleanArchitecture.Infrastructure.Ado.Sqlite;

/// <summary>
/// ADO.NET unit-of-work scope. Each <see cref="ExecuteAsync{TResult}"/> call
/// opens a connection, begins a transaction, runs the action, then commits or
/// rolls back. Dispose only cleans up resources — no implicit rollback.
/// </summary>
internal sealed class AdoUnitOfWorkScope(Func<DbConnection> connectionFactory) : IUnitOfWorkScope
{
    private readonly Func<DbConnection> _connectionFactory = connectionFactory;

    // Held during ExecuteAsync so repositories can access them.
    private DbConnection? _currentConnection;
    private DbTransaction? _currentTransaction;

    internal DbConnection Connection =>
        _currentConnection ?? throw new InvalidOperationException(
            "No active scope. Access Connection only inside ExecuteAsync.");

    internal DbTransaction Transaction =>
        _currentTransaction ?? throw new InvalidOperationException(
            "No active scope. Access Transaction only inside ExecuteAsync.");

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action, CancellationToken ct = default)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);

        _currentConnection = connection;
        _currentTransaction = transaction;
        try
        {
            var result = await action(ct).ConfigureAwait(false);
            await transaction.CommitAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
        finally
        {
            _currentConnection = null;
            _currentTransaction = null;
        }
    }

    public async Task<Result<TResult>> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<Result<TResult>>> action, CancellationToken ct = default)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);

        _currentConnection = connection;
        _currentTransaction = transaction;
        try
        {
            var result = await action(ct).ConfigureAwait(false);

            if (result.IsFailure)
            {
                await transaction.RollbackAsync(ct).ConfigureAwait(false);
                return result;
            }

            await transaction.CommitAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
        finally
        {
            _currentConnection = null;
            _currentTransaction = null;
        }
    }
}