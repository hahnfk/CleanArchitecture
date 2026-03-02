using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Infrastructure.Ado.Sqlite;

/// <summary>
/// Creates the schema and enables WAL at startup.
/// Mirrors <c>EnsureCreatedHostedService</c> from the EF Core provider.
/// </summary>
internal sealed class EnsureCreatedHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<AdoUnitOfWork>();

        if (uow.Connection.State != System.Data.ConnectionState.Open)
            await uow.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        // Create schema
        await using var schemaCmd = uow.Connection.CreateCommand();
        schemaCmd.CommandText = "CREATE TABLE IF NOT EXISTS Todos (Id TEXT NOT NULL PRIMARY KEY, Title TEXT NOT NULL, IsCompleted INTEGER NOT NULL DEFAULT 0, Version INTEGER NOT NULL DEFAULT 0);";
        await schemaCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        // Enable WAL mode for concurrent access
        await using var walCmd = uow.Connection.CreateCommand();
        walCmd.CommandText = "PRAGMA journal_mode=WAL;";
        await walCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
