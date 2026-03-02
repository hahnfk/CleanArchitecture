using System.Runtime.CompilerServices;
using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Contracts.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.Ado.Sqlite;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureAdoSqlite(
        this IServiceCollection services, IConfiguration configuration)
    {
        var options = new PersistenceOptions();
        configuration.GetSection(PersistenceOptions.SectionName).Bind(options);

        var cs = ResolveToDbFolder(
            string.IsNullOrWhiteSpace(options.ConnectionString)
                ? throw new InvalidOperationException(
                    $"Missing '{PersistenceOptions.SectionName}:{nameof(PersistenceOptions.ConnectionString)}' in configuration.")
                : options.ConnectionString,
            options.DbFolder);

        services.AddScoped<AdoUnitOfWork>(_ => new AdoUnitOfWork(new SqliteConnection(cs)));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AdoUnitOfWork>());
        services.AddScoped<ITodoRepository, AdoSqliteTodoRepository>();
        services.AddHostedService<EnsureCreatedHostedService>();

        return services;
    }

    /// <summary>
    /// Resolves a relative Data Source connection string to an absolute path
    /// inside a subfolder of this project, so that every host shares the same database file.
    /// </summary>
    private static string ResolveToDbFolder(
        string connectionString, string dbFolder,
        [CallerFilePath] string? callerFilePath = null)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource) || Path.IsPathRooted(builder.DataSource))
            return connectionString;

        var projectDir = Path.GetDirectoryName(callerFilePath)!;
        var dbDir = Path.Combine(projectDir, dbFolder);
        Directory.CreateDirectory(dbDir);

        builder.DataSource = Path.Combine(dbDir, builder.DataSource);
        return builder.ConnectionString;
    }
}
