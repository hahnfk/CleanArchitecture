using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Contracts.Persistence;
using CleanArchitecture.Infrastructure.EfCore.Sqlite.Db;
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
    /// Falls back to a runtime base directory when the project directory cannot be located.
    /// </summary>
    private static string ResolveToDbFolder(string connectionString)
    {
        const string prefix = "Data Source=";
        if (!connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return connectionString;

        var dbFileName = connectionString[prefix.Length..];
        if (Path.IsPathRooted(dbFileName))
            return connectionString;

        // Try to locate the Infrastructure.EfCore.Sqlite project directory from runtime base
        var dbDir = TryFindProjectDbFolder() ?? GetFallbackDbFolder();
        Directory.CreateDirectory(dbDir);

        return $"{prefix}{Path.Combine(dbDir, dbFileName)}";
    }

    /// <summary>
    /// Attempts to find the Db folder within the Infrastructure.EfCore.Sqlite project
    /// by walking up from the application's base directory.
    /// Returns null if the project directory cannot be located.
    /// </summary>
    private static string? TryFindProjectDbFolder()
    {
        var baseDir = AppContext.BaseDirectory;
        var currentDir = new DirectoryInfo(baseDir);

        // Walk up the directory tree to find the solution root
        while (currentDir != null)
        {
            // Look for the Infrastructure.EfCore.Sqlite project directory
            var projectDir = Path.Combine(currentDir.FullName, "CleanArchitecture.Infrastructure.EfCore.Sqlite");
            if (Directory.Exists(projectDir))
            {
                var dbDir = Path.Combine(projectDir, "Db");
                // Verify this is the correct project by checking for DependencyInjection.cs
                if (File.Exists(Path.Combine(projectDir, "DependencyInjection.cs")))
                {
                    return dbDir;
                }
            }

            currentDir = currentDir.Parent;
        }

        return null;
    }

    /// <summary>
    /// Returns a fallback database directory in the application's base directory.
    /// Used when the project directory cannot be located (e.g., in production deployments).
    /// </summary>
    private static string GetFallbackDbFolder()
    {
        return Path.Combine(AppContext.BaseDirectory, "Data");
    }
}
