using CleanArchitecture.Contracts.Persistence;
using CleanArchitecture.Infrastructure.EfCore.Sqlite;
using CleanArchitecture.Infrastructure.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Composition;

/// <summary>
/// Infrastructure composition root for persistence.
/// Presentation should only reference this project (not individual providers),
/// similar to how you did it in LLMClient.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new PersistenceOptions();
        configuration.GetSection(PersistenceOptions.SectionName).Bind(options);

        return options.Provider switch
        {
            PersistenceProvider.InMemory => services.AddInfrastructureInMemory(),
            PersistenceProvider.EfSqlite => services.AddInfrastructureEfSqlite(configuration),
            _ => throw new InvalidOperationException($"Unsupported persistence provider: {options.Provider}")
        };
    }
}
