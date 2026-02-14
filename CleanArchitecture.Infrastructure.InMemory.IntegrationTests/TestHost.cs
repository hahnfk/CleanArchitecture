using CleanArchitecture.Application;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.InMemory.IntegrationTests;

public static class TestHost
{
    /// <summary>
    /// Creates a fresh service provider per test with Application + InMemory Infrastructure.
    /// You can add/override registrations via the optional configure callback.
    /// </summary>
    public static ServiceProvider BuildServices(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();

        // Register inner layers in the same order as the app
        services.AddApplication();
        services.AddInfrastructureInMemory();

        configure?.Invoke(services);

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }
}
