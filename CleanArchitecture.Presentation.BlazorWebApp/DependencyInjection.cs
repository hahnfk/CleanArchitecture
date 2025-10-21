namespace CleanArchitecture.Presentation.BlazorWebApp;

using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure.InMemory;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddCleanArchitectureApp(this IServiceCollection services)
        => services.AddPresentation()
            .AddInfrastructureInMemory()
            .AddApplication();
}