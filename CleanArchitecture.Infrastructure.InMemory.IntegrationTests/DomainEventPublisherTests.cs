using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Identity;
using CleanArchitecture.Domain.Todos;
using CleanArchitecture.Domain.Todos.Events;
using CleanArchitecture.Infrastructure.InMemory.Events;

namespace CleanArchitecture.Infrastructure.InMemory.Tests;

public sealed class DomainEventPublisherTests
{
    [Fact]
    public async Task PublishAsync_Dispatches_To_All_Registered_Handlers()
    {
        // Arrange
        var fakeHandler = A.Fake<IDomainEventHandler<TodoCompletedDomainEvent>>();

        using var sp = TestHost.BuildServices(services =>
        {
            // Override/add our fake handler in addition to any built-in ones
            services.AddTransient<IDomainEventHandler<TodoCompletedDomainEvent>>(_ => fakeHandler);
        });

        var publisher = sp.GetRequiredService<IDomainEventPublisher>();

        // Build an aggregate that raises the event (using real domain)
        var todo = new TodoItem(TodoId.New(), "X");
        todo.ClearEvents();      // clean start
        todo.Complete();         // raises TodoCompletedDomainEvent inside domain
        var events = todo.DomainEvents.ToArray();

        // Act
        await publisher.PublishAsync(events, CancellationToken.None);

        // Assert
        A.CallTo(() => fakeHandler.HandleAsync(
                A<TodoCompletedDomainEvent>.That.Matches(e => e.TodoId.Equals(todo.Id)),
                A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task PublishAsync_Ignores_Empty_Event_List()
    {
        // Arrange
        using var sp = TestHost.BuildServices();
        var publisher = sp.GetRequiredService<IDomainEventPublisher>();

        // Act
        await publisher.PublishAsync(Enumerable.Empty<IDomainEvent>(), CancellationToken.None);

        // Assert
        // nothing to assert: test should simply complete without exceptions
        Assert.True(true);
    }
}
