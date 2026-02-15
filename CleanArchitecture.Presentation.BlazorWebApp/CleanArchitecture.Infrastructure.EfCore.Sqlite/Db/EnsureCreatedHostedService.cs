using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Infrastructure.EfCore.Sqlite.Db;

internal sealed class EnsureCreatedHostedService : IHostedService
{
    private readonly AppDbContext _db;

    public EnsureCreatedHostedService(AppDbContext db) => _db = db;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _db.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        // Enable WAL mode so concurrent readers are not blocked by a single writer.
        await _db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken)
            .ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
