using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskPage.Application.Common.Interfaces;
using TaskPage.Infrastructure.Persistence;
using TaskPage.Infrastructure.Repositories;

namespace TaskPage.Infrastructure;

/// <summary>
/// Registers the infrastructure services: SQL Server context and repositories.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Adds SQL Server persistence and the repository implementations.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <param name="configuration">Application configuration with the connection string.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
