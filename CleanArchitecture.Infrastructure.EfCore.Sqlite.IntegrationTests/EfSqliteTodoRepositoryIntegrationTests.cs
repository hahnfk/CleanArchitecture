using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;

namespace CleanArchitecture.Infrastructure.EfCore.Sqlite.IntegrationTests;

public sealed class EfSqliteTodoRepositoryIntegrationTests(EfSqliteTestHost host) : IClassFixture<EfSqliteTestHost>
{
    private readonly EfSqliteTestHost _host = host;

    [Fact]
    public async Task Add_And_List_Persists_Items()
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
    public async Task GetById_Returns_Null_When_Missing()
    {
        var result = await _host.Todos.GetByIdAsync(new TodoId(Guid.NewGuid()));

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_Persists_Changes()
    {
        var id = new TodoId(Guid.NewGuid());
        var item = new TodoItem(id, "Initial");

        await _host.Todos.AddAsync(item);
        await _host.Uow.SaveChangesAsync();

        item.Rename("Updated");
        item.Complete();

        await _host.Todos.UpdateAsync(item);
        await _host.Uow.SaveChangesAsync();

        var loaded = await _host.Todos.GetByIdAsync(id);

        Assert.NotNull(loaded);
        Assert.Equal("Updated", loaded!.Title);
        Assert.True(loaded.IsCompleted);
    }

    [Fact]
    public async Task Delete_Returns_False_When_Missing()
    {
        var deleted = await _host.Todos.DeleteAsync(new TodoId(Guid.NewGuid()));
        await _host.Uow.SaveChangesAsync();

        Assert.False(deleted);
    }

    [Fact]
    public async Task Delete_Removes_Item()
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
}
