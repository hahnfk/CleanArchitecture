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
        var svc = scope.ServiceProvider;

        // Act
        var repo = svc.GetService<ITodoRepository>();
        var uow = svc.GetService<IUnitOfWork>();
        var pub = svc.GetService<IDomainEventPublisher>();

        // Assert
        Assert.NotNull(repo);
        Assert.NotNull(uow);
        Assert.NotNull(pub);
    }
}
