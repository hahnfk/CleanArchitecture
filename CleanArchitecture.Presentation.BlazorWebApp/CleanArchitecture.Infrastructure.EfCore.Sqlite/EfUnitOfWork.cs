using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Infrastructure.EfCore.Sqlite.Db;

namespace CleanArchitecture.Infrastructure.EfCore.Sqlite;

internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public EfUnitOfWork(AppDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct).ConfigureAwait(false);
}
