using Microsoft.EntityFrameworkCore;
using TaskPage.Domain.Entities;

namespace TaskPage.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the TaskPage schema. The physical schema is
/// owned by database/schema.sql; the mappings keep the model aligned with it.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>Creates the context with its EF Core options.</summary>
    /// <param name="options">Options that describe the SQL Server connection.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>Users table.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Tasks table.</summary>
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    /// <summary>Applies the entity configurations and maps the JSON functions.</summary>
    /// <param name="modelBuilder">Builder used to shape the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        var jsonValue = modelBuilder.HasDbFunction(
            typeof(SqlServerJsonFunctions).GetMethod(nameof(SqlServerJsonFunctions.JsonValue))!);
        jsonValue.HasName("JSON_VALUE");
        jsonValue.IsBuiltIn();
    }
}
