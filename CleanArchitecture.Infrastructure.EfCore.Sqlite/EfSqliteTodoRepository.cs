using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using CleanArchitecture.Infrastructure.EfCore.Sqlite.Db;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.EfCore.Sqlite;

internal sealed class EfSqliteTodoRepository : ITodoRepository
{
    private readonly AppDbContext _db;

    public EfSqliteTodoRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(TodoItem todo, CancellationToken ct = default)
    {
        var row = ToRow(todo);
        await _db.Todos.AddAsync(row, ct).ConfigureAwait(false);
    }

    public async Task AddRangeAsync(IEnumerable<TodoItem> todos, CancellationToken ct = default)
    {
        var rows = todos.Select(ToRow).ToList();
        await _db.Todos.AddRangeAsync(rows, ct).ConfigureAwait(false);
    }

    public async Task<TodoItem?> GetByIdAsync(TodoId id, CancellationToken ct = default)
    {
        var row = await _db.Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id.Value, ct)
            .ConfigureAwait(false);

        return row is null ? null : ToDomain(row);
    }

    public async Task<IReadOnlyList<TodoItem>> ListAsync(CancellationToken ct = default)
    {
        var rows = await _db.Todos
            .AsNoTracking()
            .OrderBy(x => x.Title)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return rows.Select(ToDomain).ToList();
    }

    public Task UpdateAsync(TodoItem todo, CancellationToken ct = default)
    {
        var row = ToRow(todo);

        var existingEntry = _db.ChangeTracker.Entries<TodoRow>()
            .FirstOrDefault(e => e.Entity.Id == row.Id);

        if (existingEntry is not null)
        {
            existingEntry.CurrentValues.SetValues(row);
            existingEntry.State = EntityState.Modified;
            existingEntry.Property(x => x.Version).OriginalValue = todo.OriginalVersion;
        }
        else
        {
            _db.Attach(row);
            var entry = _db.Entry(row);
            entry.State = EntityState.Modified;
            entry.Property(x => x.Version).OriginalValue = todo.OriginalVersion;
        }

        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(TodoId id, CancellationToken ct = default)
    {
        var existing = await _db.Todos.FirstOrDefaultAsync(x => x.Id == id.Value, ct).ConfigureAwait(false);
        if (existing is null) return false;

        _db.Todos.Remove(existing);
        return true;
    }

    private static TodoRow ToRow(TodoItem todo)
        => new()
        {
            Id = todo.Id.Value,
            Title = todo.Title,
            IsCompleted = todo.IsCompleted,
            Version = todo.Version
        };

    private static TodoItem ToDomain(TodoRow row)
        => TodoItem.Rehydrate(new TodoId(row.Id), row.Title, row.IsCompleted, row.Version);
}
