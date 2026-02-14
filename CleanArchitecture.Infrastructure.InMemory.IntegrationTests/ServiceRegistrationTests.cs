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
        using var scope = sp.CreateScope();

        // Act
        var repo = scope.ServiceProvider.GetService<ITodoRepository>();
        var uow = scope.ServiceProvider.GetService<IUnitOfWork>();
        var pub = scope.ServiceProvider.GetService<IDomainEventPublisher>();

        // Assert
        Assert.NotNull(repo);
        Assert.NotNull(uow);
        Assert.NotNull(pub);
    }
}
