using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Infrastructure.InMemory.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.InMemory.IntegrationTests;

public sealed class ServiceRegistrationTests
{
    [Fact]
    public void AddInfrastructureInMemory_Resolves_Core_Ports()
    {
        // Arrange
        using var sp = TestHost.BuildServices();

        // Act
        var repo = sp.GetService<ITodoRepository>();
        var uow = sp.GetService<IUnitOfWork>();
        var pub = sp.GetService<IDomainEventPublisher>();

        // Assert
        Assert.NotNull(repo);
        Assert.NotNull(uow);
        Assert.NotNull(pub);
    }
}
