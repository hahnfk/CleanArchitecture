using System.Data.Common;
using CleanArchitecture.Application.Abstractions;

namespace CleanArchitecture.Infrastructure.Ado.Sqlite;

/// <summary>
/// ADO.NET unit of work backed by a <see cref="DbTransaction"/>.
/// Each scoped lifetime gets its own connection + transaction.
/// <see cref="SaveChangesAsync"/> commits; disposal rolls back if uncommitted.
/// </summary>
internal sealed class AdoUnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly DbConnection _connection;
    private DbTransaction? _transaction;
    private bool _committed;
    private bool _disposed;

    public AdoUnitOfWork(DbConnection connection)
    {
        _connection = connection;
    }

    /// <summary>The shared connection for this unit of work scope.</summary>
    internal DbConnection Connection => _connection;

    /// <summary>The current transaction, lazily started on first use.</summary>
    internal async Task<DbTransaction> GetTransactionAsync(CancellationToken ct = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct).ConfigureAwait(false);

        return _transaction ??= await _connection.BeginTransactionAsync(ct).ConfigureAwait(false);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            return 0;

        await _transaction.CommitAsync(ct).ConfigureAwait(false);
        await _transaction.DisposeAsync().ConfigureAwait(false);
        _transaction = null;
        _committed = true;

        return 1;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_transaction is not null && !_committed)
        {
            try { await _transaction.RollbackAsync().ConfigureAwait(false); }
            catch { /* best effort */ }
        }

        if (_transaction is not null)
            await _transaction.DisposeAsync().ConfigureAwait(false);

        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}
