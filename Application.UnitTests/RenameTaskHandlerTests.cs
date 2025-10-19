using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.UseCases.Todos.Commands.RenameTodo;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using CleanArchitecture.Domain.Todos.Events;
using FakeItEasy;

namespace CleanArchitecture.Application.Tests.Todos;

public sealed class RenameTodoHandlerTests
{
    private static TodoItem NewTodo(string title = "Old")
        => new(new TodoId(Guid.NewGuid()), title);

    [Fact]
    public async Task Handle_Renames_Todo_Publishes_Renamed_Event_And_Clears_DomainEvents()
    {
        // Arrange
        var repo = A.Fake<ITodoRepository>();
        var uow = A.Fake<IUnitOfWork>();
        var pub = A.Fake<IDomainEventPublisher>();

        var Todo = NewTodo("Old");
        Todo.ClearEvents();
        var id = Todo.Id;

        A.CallTo(() => repo.GetByIdAsync(id, A<CancellationToken>._))
            .Returns(Todo);

        var sut = new RenameTodoHandler(repo, uow, pub);

        // Act
        await sut.Handle(new RenameTodoRequest { TodoId = id.Value.ToString(), NewTitle = "New" }, CancellationToken.None);

        // Assert
        Assert.Equal("New", Todo.Title);

        A.CallTo(() => repo.UpdateAsync(Todo, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => uow.SaveChangesAsync(A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => pub.PublishAsync(
                A<System.Collections.Generic.IEnumerable<IDomainEvent>>.That.Matches(evts =>
                    evts.OfType<TodoRenamedDomainEvent>().Any(e => e.OldTitle == "Old" && e.NewTitle == "New")),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        Assert.Empty(Todo.DomainEvents);
    }

    [Fact]
    public async Task Handle_With_Same_Title_Is_Idempotent_Publishes_Nothing()
    {
        // Arrange
        var repo = A.Fake<ITodoRepository>();
        var uow = A.Fake<IUnitOfWork>();
        var pub = A.Fake<IDomainEventPublisher>();

        var Todo = NewTodo("Same");
        Todo.ClearEvents();
        var id = Todo.Id;

        A.CallTo(() => repo.GetByIdAsync(id, A<CancellationToken>._))
            .Returns(Todo);

        var sut = new RenameTodoHandler(repo, uow, pub);

        // Act
        await sut.Handle(new RenameTodoRequest { TodoId = id.Value.ToString(), NewTitle = "Same" }, CancellationToken.None);

        // Assert
        // No change, so no publish expected (depends on your handler logic; adjust if you still call Publish with empty list)
        A.CallTo(() => pub.PublishAsync(
                A<System.Collections.Generic.IEnumerable<IDomainEvent>>.That.Matches(evts => !evts.Any()),
                A<CancellationToken>._)).MustNotHaveHappened();

        Assert.Empty(Todo.DomainEvents);
        Assert.Equal("Same", Todo.Title);
    }

    [Fact]
    public async Task Handle_With_Invalid_Id_Does_Nothing()
    {
        // Arrange
        var repo = A.Fake<ITodoRepository>();
        var uow = A.Fake<IUnitOfWork>();
        var pub = A.Fake<IDomainEventPublisher>();

        var sut = new RenameTodoHandler(repo, uow, pub);

        // Act
        await sut.Handle(new RenameTodoRequest { TodoId = "not-a-guid", NewTitle = "X" }, CancellationToken.None);

        // Assert: explizit pro Methode prüfen, dass nichts aufgerufen wurde
        A.CallTo(() => repo.GetByIdAsync(A<TodoId>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => repo.UpdateAsync(A<TodoItem>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => uow.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => pub.PublishAsync(A<System.Collections.Generic.IEnumerable<IDomainEvent>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
