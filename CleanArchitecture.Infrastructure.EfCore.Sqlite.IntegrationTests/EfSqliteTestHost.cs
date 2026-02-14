namespace CleanArchitecture.Infrastructure.EfCore.Sqlite.IntegrationTests;

using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Contracts.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Creates a fresh, file-based SQLite database per test fixture.
/// We intentionally avoid SQLite in-memory mode here, because that requires a shared open connection.
/// </summary>
public sealed class EfSqliteTestHost : IAsyncLifetime
{
    private readonly string _dbPath;
    private IServiceScope? _scope;

    public EfSqliteTestHost()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"cleanarchitecture-tests-{Guid.NewGuid():N}.db");
    }

    public ServiceProvider ServiceProvider { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var configuration = BuildConfiguration(_dbPath);

        var services = new ServiceCollection();
        services.AddInfrastructureEfSqlite(configuration);

        ServiceProvider = services.BuildServiceProvider(validateScopes: true);

        // Ensure schema exists (hosted service is not started in plain DI scenarios).
        using (var scope = ServiceProvider.CreateScope())
        {
            await EnsureCreatedAsync(scope.ServiceProvider).ConfigureAwait(false);
        }

        // Create a shared scope for the lifetime of the test host.
        _scope = ServiceProvider.CreateScope();
    }

    public Task DisposeAsync()
    {
        try
        {
            _scope?.Dispose();
            ServiceProvider.Dispose();
        }
        finally
        {
            TryDelete(_dbPath);
        }

        return Task.CompletedTask;
    }

    public ITodoRepository Todos => _scope!.ServiceProvider.GetRequiredService<ITodoRepository>();
    public IUnitOfWork Uow => _scope!.ServiceProvider.GetRequiredService<IUnitOfWork>();

    private static IConfiguration BuildConfiguration(string dbPath)
    {
        var values = new Dictionary<string, string?>
        {
            [$"{PersistenceOptions.SectionName}:Provider"] = PersistenceProvider.EfSqlite.ToString(),
            [$"{PersistenceOptions.SectionName}:ConnectionString"] = $"Data Source={dbPath}"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static async Task EnsureCreatedAsync(IServiceProvider sp)
    {
        // AppDbContext is internal; resolve it via reflection to avoid exposing it in the public surface.
        var ctxType = Type.GetType("CleanArchitecture.Infrastructure.EfCore.Sqlite.Db.AppDbContext, CleanArchitecture.Infrastructure.EfCore.Sqlite");
        if (ctxType is null)
            throw new InvalidOperationException("Could not resolve AppDbContext type. Check assembly/name changes.");

        dynamic? ctx = sp.GetService(ctxType);
        if (ctx is null)
            throw new InvalidOperationException("Could not resolve AppDbContext from DI container.");

        await ctx.Database.EnsureCreatedAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Intentionally ignored (best effort cleanup).
        }
    }
}
