using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTodo;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using CleanArchitecture.Domain.Todos.Events;
using FakeItEasy;

namespace CleanArchitecture.Application.UnitTests.UseCases;

public sealed class CompleteTodoHandlerTests
{
    private static TodoItem NewTodo(string title = "t1")
         => new(new TodoId(Guid.NewGuid()), title);

    [Fact]
    public async Task Handle_Completes_Todo_Publishes_Event_And_Clears_DomainEvents()
    {
        // Arrange
        var repo = A.Fake<ITodoRepository>();
        var uow = A.Fake<IUnitOfWork>();
        var pub = A.Fake<IDomainEventPublisher>();

        var todo = NewTodo("X");
        todo.ClearEvents(); // start clean
        var id = todo.Id;

        A.CallTo(() => repo.GetByIdAsync(id, A<CancellationToken>._)).Returns(todo);

        var sut = new CompleteTodoHandler(repo, uow, pub);

        // Act
        await sut.Handle(new CompleteTodoRequest { Id = id.Value.ToString() }, CancellationToken.None);

        // Assert
        Assert.True(todo.IsCompleted);

        A.CallTo(() => repo.UpdateAsync(todo, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => uow.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        A.CallTo(() => pub.PublishAsync(
                A<IEnumerable<IDomainEvent>>.That.Matches(evts =>
                    evts.Any(e => e is TodoCompletedDomainEvent)),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        // Handler sollte Events danach leeren
        Assert.Empty(todo.DomainEvents);
    }

    [Fact]
    public async Task Handle_With_Invalid_Id_Does_Not_Call_Repo_UnitOfWork_Or_Publisher()
    {
        // Arrange
        var repo = A.Fake<ITodoRepository>();
        var uow = A.Fake<IUnitOfWork>();
        var pub = A.Fake<IDomainEventPublisher>();

        var sut = new CompleteTodoHandler(repo, uow, pub);

        // Act
        await sut.Handle(new CompleteTodoRequest { Id = "not-a-guid" }, CancellationToken.None);

        // Assert
        A.CallTo(() => repo.GetByIdAsync(A<TodoId>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => repo.UpdateAsync(A<TodoItem>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => uow.SaveChangesAsync(A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => pub.PublishAsync(A<IEnumerable<IDomainEvent>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
