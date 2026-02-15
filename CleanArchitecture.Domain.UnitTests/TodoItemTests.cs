using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using CleanArchitecture.Domain.Todos.Events;

namespace CleanArchitecture.Domain.UnitTests;

public sealed class TodoItemTests
{
    private static TodoItem NewTodo(string title = "First title")
        => new(new TodoId(Guid.NewGuid()), title);

    [Fact]
    public void Rename_Emits_TodoRenamedDomainEvent_When_Title_Changes()
    {
        // Arrange
        var Todo = NewTodo("Old");
        Todo.ClearEvents();

        // Act
        Todo.Rename("New");

        // Assert
        Assert.Equal("New", Todo.Title);
        var ev = Todo.DomainEvents.OfType<TodoRenamedDomainEvent>().Single();
        Assert.Equal(Todo.Id, ev.TodoId);
        Assert.Equal("Old", ev.OldTitle);
        Assert.Equal("New", ev.NewTitle);
    }

    [Fact]
    public void Rename_Is_Idempotent_For_Same_Title()
    {
        // Arrange
        var Todo = NewTodo("Same");
        Todo.ClearEvents();

        // Act
        Todo.Rename("Same");

        // Assert
        Assert.Empty(Todo.DomainEvents);
        Assert.Equal("Same", Todo.Title);
    }

    [Fact]
    public void Complete_Emits_TodoCompletedDomainEvent_Once()
    {
        // Arrange
        var Todo = NewTodo("t1");
        Todo.ClearEvents();

        // Act
        Todo.Complete();
        Todo.Complete(); // second call should be idempotent

        // Assert
        var events = Todo.DomainEvents.OfType<TodoCompletedDomainEvent>().ToList();
        Assert.True(Todo.IsCompleted);
        Assert.Single(events);
    }

    [Fact]
    public void Reopen_Emits_TodoReopenedDomainEvent_Once()
    {
        // Arrange
        var Todo = NewTodo();
        Todo.Complete();
        Todo.ClearEvents();

        // Act
        Todo.Reopen();
        Todo.Reopen(); // idempotent

        // Assert
        Assert.False(Todo.IsCompleted);
        var events = Todo.DomainEvents.OfType<TodoReopenedDomainEvent>().ToList();
        Assert.Single(events);
        Assert.Equal(Todo.Id, events [0].TodoId);
    }

    [Fact]
    public void Version_Increments_On_Mutations()
    {
        // Arrange
        var Todo = NewTodo("V1");
        var v0 = Todo.Version;

        // Act
        Todo.Rename("V2");
        var v1 = Todo.Version;
        Todo.Complete();
        var v2 = Todo.Version;

        // Assert
        Assert.True(v1 >= v0 + 1);
        Assert.True(v2 >= v1 + 1);
    }

    // --- OriginalVersion tests (concurrency-token fix) ---

    [Fact]
    public void NewItem_OriginalVersion_Is_Zero()
    {
        var todo = NewTodo("Fresh");

        Assert.Equal(0, todo.OriginalVersion);
    }

    [Fact]
    public void Rehydrate_Sets_OriginalVersion()
    {
        var todo = TodoItem.Rehydrate(
            new TodoId(Guid.NewGuid()), "Title", false, version: 5);

        Assert.Equal(5, todo.Version);
        Assert.Equal(5, todo.OriginalVersion);
    }

    [Fact]
    public void OriginalVersion_Unchanged_After_Mutations()
    {
        // Arrange – simulate a rehydrated aggregate at version 3
        var todo = TodoItem.Rehydrate(
            new TodoId(Guid.NewGuid()), "Title", false, version: 3);

        // Act – multiple mutations bump Version but must NOT touch OriginalVersion
        todo.Rename("Renamed");
        todo.Complete();
        todo.Reopen();

        // Assert
        Assert.Equal(3, todo.OriginalVersion);  // stays at loaded value
        Assert.Equal(6, todo.Version);           // incremented 3 times
    }
}
