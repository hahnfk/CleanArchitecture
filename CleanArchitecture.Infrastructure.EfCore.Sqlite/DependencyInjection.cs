using System.Runtime.CompilerServices;
using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Contracts.Persistence;
using CleanArchitecture.Infrastructure.EfCore.Sqlite.Db;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.EfCore.Sqlite;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureEfSqlite(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new PersistenceOptions();
        configuration.GetSection(PersistenceOptions.SectionName).Bind(options);

        var cs = options.ConnectionString;
        if (string.IsNullOrWhiteSpace(cs))
        {
            cs = "Data Source=cleanarchitecture.db";
        }

        cs = ResolveToDbFolder(cs);

        services.AddDbContext<AppDbContext>(o => o.UseSqlite(cs));

        // Simple DB bootstrap for demos. For production: migrations + controlled rollout.
        services.AddHostedService<EnsureCreatedHostedService>();

        // Persistence ports
        services.AddScoped<ITodoRepository, EfSqliteTodoRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Resolves a relative "Data Source=&lt;file&gt;" connection string to an absolute path
    /// inside the <c>Db</c> subfolder of this project, so that every host (WPF, Blazor, …)
    /// shares the same database file during development.
    /// Preserves any additional connection string parameters (e.g., Cache, Mode, etc.).
    /// </summary>
    private static string ResolveToDbFolder(string connectionString, [CallerFilePath] string? callerFilePath = null)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        
        // If no Data Source is specified or it's already an absolute path, return as-is
        if (string.IsNullOrWhiteSpace(builder.DataSource) || Path.IsPathRooted(builder.DataSource))
            return connectionString;

        var projectDir = Path.GetDirectoryName(callerFilePath)!;
        var dbDir = Path.Combine(projectDir, "Db");
        Directory.CreateDirectory(dbDir);

        builder.DataSource = Path.Combine(dbDir, builder.DataSource);
        return builder.ConnectionString;
    }
}
