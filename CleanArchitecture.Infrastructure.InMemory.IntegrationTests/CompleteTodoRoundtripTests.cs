using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Application.UseCases.Todos.Commands.CompleteTodo;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using CleanArchitecture.Domain.Todos.Events;
using CleanArchitecture.Infrastructure.Composition.DomainEvents;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.InMemory.Tests.Integration.Todos;

public sealed class CompleteTodoRoundtripTests
{
    [Fact]
    public async Task Completing_Todo_Persists_And_Publishes_Event()
    {
        // Arrange: baue SP, hänge einen Fake-Handler zusätzlich rein
        var fakeHandler = A.Fake<IDomainEventHandler<TodoCompletedDomainEvent>>();
        using var sp = TestHost.BuildServices(services =>
        {
            services.AddTransient<IDomainEventHandler<TodoCompletedDomainEvent>>(_ => fakeHandler);
        });
        using var scope = sp.CreateScope();
        var svc = scope.ServiceProvider;

        var repo = svc.GetRequiredService<ITodoRepository>();
        var uow = svc.GetRequiredService<IUnitOfWork>();
        var publisher = svc.GetRequiredService<IDomainEventPublisher>();
        var handler = new CompleteTodoHandler(repo, uow, publisher);

        var todo = new TodoItem(TodoId.New(), "X");
        await repo.AddAsync(todo, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None);

        // Act: UseCase ausführen
        await handler.Handle(new CompleteTodoRequest { Id = todo.Id.ToString() }, CancellationToken.None);

        // Assert: Zustand + Publish
        var reloaded = await repo.GetByIdAsync(todo.Id, CancellationToken.None);
        Assert.NotNull(reloaded);
        Assert.True(reloaded!.IsCompleted);

        A.CallTo(() => fakeHandler.HandleAsync(
                A<TodoCompletedDomainEvent>.That.Matches(e => e.TodoId.Equals(todo.Id)),
                A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}
