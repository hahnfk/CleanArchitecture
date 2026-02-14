using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.EfCore.Sqlite.Db;

internal sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoRow> Todos => Set<TodoRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoRow>(b =>
        {
            b.ToTable("Todos");
            b.HasKey(x => x.Id);

            b.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(400);

            b.Property(x => x.IsCompleted)
                .IsRequired();

            // Optimistic concurrency token (optional) - aligns with Domain's AggregateRoot.Version
            b.Property(x => x.Version)
                .IsRequired()
                .IsConcurrencyToken();
        });
    }
}
