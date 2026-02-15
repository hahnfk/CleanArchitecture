using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.EfCore.Sqlite.IntegrationTests;

public sealed class EfSqliteTodoRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly EfSqliteTestHost _host = new();

    public Task InitializeAsync() => _host.InitializeAsync();
    public Task DisposeAsync() => _host.DisposeAsync();

    [Fact]
    public async Task AddAndListPersistsItems()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        await _host.Todos.AddAsync(new TodoItem(new TodoId(guid1), "Write tests"));
        await _host.Todos.AddAsync(new TodoItem(new TodoId(guid2), "Run tests"));
        await _host.Uow.SaveChangesAsync();

        var list = await _host.Todos.ListAsync();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.Id.Value == guid1);
        Assert.Contains(list, x => x.Id.Value == guid2);
    }

    [Fact]
    public async Task GetByIdReturnsNullWhenMissing()
    {
        var result = await _host.Todos.GetByIdAsync(new TodoId(Guid.NewGuid()));

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdatePersistsChanges()
    {
        var id = new TodoId(Guid.NewGuid());
        await _host.Todos.AddAsync(new TodoItem(id, "Initial"));
        await _host.Uow.SaveChangesAsync();

        // Reload from persistence so OriginalVersion matches the DB
        var item = await _host.Todos.GetByIdAsync(id);
        Assert.NotNull(item);

        item!.Rename("Updated");
        item.Complete();

        await _host.Todos.UpdateAsync(item);
        await _host.Uow.SaveChangesAsync();

        var loaded = await _host.Todos.GetByIdAsync(id);

        Assert.NotNull(loaded);
        Assert.Equal("Updated", loaded!.Title);
        Assert.True(loaded.IsCompleted);
    }

    [Fact]
    public async Task DeleteReturnsFalseWhenMissing()
    {
        var deleted = await _host.Todos.DeleteAsync(new TodoId(Guid.NewGuid()));
        await _host.Uow.SaveChangesAsync();

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteRemovesItem()
    {
        var id = new TodoId(Guid.NewGuid());
        await _host.Todos.AddAsync(new TodoItem(id, "Temp"));
        await _host.Uow.SaveChangesAsync();

        var deleted = await _host.Todos.DeleteAsync(id);
        await _host.Uow.SaveChangesAsync();

        Assert.True(deleted);

        var loaded = await _host.Todos.GetByIdAsync(id);
        Assert.Null(loaded);
    }

    // --- Concurrency-token tests (OriginalVersion fix) ---

    [Fact]
    public async Task Rename_AfterLoad_DoesNotThrowConcurrencyException()
    {
        // Arrange – insert and reload so the item is rehydrated with OriginalVersion
        var id = new TodoId(Guid.NewGuid());
        await _host.Todos.AddAsync(new TodoItem(id, "Before"));
        await _host.Uow.SaveChangesAsync();

        var loaded = await _host.Todos.GetByIdAsync(id);
        Assert.NotNull(loaded);

        // Act – mutate and save (this threw DbUpdateConcurrencyException before the fix)
        loaded!.Rename("After");
        await _host.Todos.UpdateAsync(loaded);
        await _host.Uow.SaveChangesAsync();

        // Assert
        var reloaded = await _host.Todos.GetByIdAsync(id);
        Assert.Equal("After", reloaded!.Title);
    }

    [Fact]
    public async Task MultipleUpdateRoundTrips_VersionIncrements()
    {
        // Arrange
        var id = new TodoId(Guid.NewGuid());
        await _host.Todos.AddAsync(new TodoItem(id, "R0"));
        await _host.Uow.SaveChangesAsync();

        // Round-trip 1: rename
        var v1 = await _host.Todos.GetByIdAsync(id);
        v1!.Rename("R1");
        await _host.Todos.UpdateAsync(v1);
        await _host.Uow.SaveChangesAsync();

        // Round-trip 2: complete
        var v2 = await _host.Todos.GetByIdAsync(id);
        v2!.Complete();
        await _host.Todos.UpdateAsync(v2);
        await _host.Uow.SaveChangesAsync();

        // Assert – both round-trips persisted correctly
        var final = await _host.Todos.GetByIdAsync(id);
        Assert.Equal("R1", final!.Title);
        Assert.True(final.IsCompleted);
        Assert.True(final.Version > v1.OriginalVersion,
            "Version should have increased across round-trips.");
    }

    [Fact]
    public async Task ConcurrentUpdate_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange – create the item
        var id = new TodoId(Guid.NewGuid());
        await _host.Todos.AddAsync(new TodoItem(id, "Original"));
        await _host.Uow.SaveChangesAsync();

        // Load TWO independent copies (simulating two concurrent requests)
        var copy1 = await _host.Todos.GetByIdAsync(id);
        var copy2 = await _host.Todos.GetByIdAsync(id);
        Assert.NotNull(copy1);
        Assert.NotNull(copy2);

        // First writer succeeds
        copy1!.Rename("Writer1");
        await _host.Todos.UpdateAsync(copy1);
        await _host.Uow.SaveChangesAsync();

        // Second writer must fail with a concurrency conflict
        copy2!.Rename("Writer2");
        await _host.Todos.UpdateAsync(copy2);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => _host.Uow.SaveChangesAsync());
    }
}
