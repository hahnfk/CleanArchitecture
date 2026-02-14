using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Infrastructure.EfCore.Sqlite.Db;
using CleanArchitecture.Contracts.Persistence;
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
            // Safe default for local dev; override via appsettings.json / user secrets.
            cs = "Data Source=cleanarchitecture.db";
        }

        services.AddDbContext<AppDbContext>(o => o.UseSqlite(cs));

        // Simple DB bootstrap for demos. For production: migrations + controlled rollout.
        services.AddHostedService<EnsureCreatedHostedService>();

        services.AddScoped<ITodoRepository, EfSqliteTodoRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
