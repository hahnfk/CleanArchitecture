using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.InMemory.IntegrationTests;

public sealed class InMemoryTodoRepositoryTests
{
    [Fact]
    public async Task Add_Get_List_Update_Delete_Work_EndToEnd()
    {
        // Arrange
        using var sp = TestHost.BuildServices();
        var repo = sp.GetRequiredService<ITodoRepository>();
        var uow = sp.GetRequiredService<IUnitOfWork>();

        var id = TodoId.New();
        var todo = new TodoItem(id, "write tests");

        // Act: Add
        await repo.AddAsync(todo, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None);

        // Assert: Get
        var loaded = await repo.GetByIdAsync(id, CancellationToken.None);
        Assert.NotNull(loaded);
        Assert.Equal("write tests", loaded!.Title);
        Assert.False(loaded.IsCompleted);

        // Act: Update (Complete)
        loaded.Complete();
        await repo.UpdateAsync(loaded, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None);

        // Assert: List shows completed item
        var list = await repo.ListAsync(CancellationToken.None);
        var row = list.SingleOrDefault(x => x.Id == id);
        Assert.NotNull(row);
        Assert.True(row!.IsCompleted);

        // Act: Delete
        await repo.DeleteAsync(id, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None);

        // Assert: Gone
        var missing = await repo.GetByIdAsync(id, CancellationToken.None);
        Assert.Null(missing);
    }
}
