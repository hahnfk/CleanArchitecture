using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using CleanArchitecture.Domain.Todos.Events;

namespace CleanArchitecture.Domain.UnitTests.Todos;

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
    public void Reopen_Toggles_From_Completed_And_Is_Idempotent()
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
        // Adjust if you emit a TodoReopenedDomainEvent
        Assert.Empty(Todo.DomainEvents);
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
}
