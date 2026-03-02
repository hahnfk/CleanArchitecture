using System.Data.Common;
using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using Microsoft.Data.Sqlite;

namespace CleanArchitecture.Infrastructure.Ado.Sqlite;

/// <summary>
/// Handwritten ADO.NET repository for <see cref="TodoItem"/>.
/// No ORM - just raw SQL with parameterised queries.
/// Optimistic concurrency is enforced via a WHERE Version = @OriginalVersion clause.
/// </summary>
internal sealed class AdoSqliteTodoRepository : ITodoRepository
{
    private readonly AdoUnitOfWork _uow;

    public AdoSqliteTodoRepository(AdoUnitOfWork uow) => _uow = uow;

    public async Task AddAsync(TodoItem todo, CancellationToken ct = default)
    {
        var tx = await _uow.GetTransactionAsync(ct).ConfigureAwait(false);
        await using var cmd = _uow.Connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "INSERT INTO Todos (Id, Title, IsCompleted, Version) VALUES (@Id, @Title, @IsCompleted, @Version);";

        AddParameters(cmd, todo.Id.Value, todo.Title, todo.IsCompleted, todo.Version);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    public async Task AddRangeAsync(IEnumerable<TodoItem> todos, CancellationToken ct = default)
    {
        foreach (var todo in todos)
            await AddAsync(todo, ct).ConfigureAwait(false);
    }

    public async Task<TodoItem?> GetByIdAsync(TodoId id, CancellationToken ct = default)
    {
        var tx = await _uow.GetTransactionAsync(ct).ConfigureAwait(false);
        await using var cmd = _uow.Connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT Id, Title, IsCompleted, Version FROM Todos WHERE Id = @Id;";
        cmd.Parameters.Add(new SqliteParameter("@Id", id.Value));

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        return await reader.ReadAsync(ct).ConfigureAwait(false)
            ? ReadTodoItem(reader)
            : null;
    }

    public async Task<IReadOnlyList<TodoItem>> ListAsync(CancellationToken ct = default)
    {
        var tx = await _uow.GetTransactionAsync(ct).ConfigureAwait(false);
        await using var cmd = _uow.Connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT Id, Title, IsCompleted, Version FROM Todos ORDER BY Title;";

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        var results = new List<TodoItem>();
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
            results.Add(ReadTodoItem(reader));

        return results;
    }

    public async Task UpdateAsync(TodoItem todo, CancellationToken ct = default)
    {
        var tx = await _uow.GetTransactionAsync(ct).ConfigureAwait(false);
        await using var cmd = _uow.Connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "UPDATE Todos SET Title = @Title, IsCompleted = @IsCompleted, Version = @Version WHERE Id = @Id AND Version = @OriginalVersion;";

        AddParameters(cmd, todo.Id.Value, todo.Title, todo.IsCompleted, todo.Version);
        cmd.Parameters.Add(new SqliteParameter("@OriginalVersion", todo.OriginalVersion));

        var affected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (affected == 0)
            throw new DbConcurrencyException(
                $"Concurrency conflict updating TodoItem {todo.Id.Value}. " +
                $"Expected Version={todo.OriginalVersion}, but the row was modified or deleted.");
    }

    public async Task<bool> DeleteAsync(TodoId id, CancellationToken ct = default)
    {
        var tx = await _uow.GetTransactionAsync(ct).ConfigureAwait(false);
        await using var cmd = _uow.Connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "DELETE FROM Todos WHERE Id = @Id;";
        cmd.Parameters.Add(new SqliteParameter("@Id", id.Value));

        return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false) > 0;
    }

    private static void AddParameters(DbCommand cmd, Guid id, string title, bool isCompleted, long version)
    {
        cmd.Parameters.Add(new SqliteParameter("@Id", id));
        cmd.Parameters.Add(new SqliteParameter("@Title", title));
        cmd.Parameters.Add(new SqliteParameter("@IsCompleted", isCompleted));
        cmd.Parameters.Add(new SqliteParameter("@Version", version));
    }

    private static TodoItem ReadTodoItem(DbDataReader reader)
        => TodoItem.Rehydrate(
            new TodoId(reader.GetGuid(0)),
            reader.GetString(1),
            reader.GetBoolean(2),
            reader.GetInt64(3));
}
