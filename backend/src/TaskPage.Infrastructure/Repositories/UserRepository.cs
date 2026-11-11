using Microsoft.EntityFrameworkCore;
using TaskPage.Application.Common.Interfaces;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Users.DTOs;
using TaskPage.Domain.Entities;
using TaskPage.Infrastructure.Persistence;

namespace TaskPage.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the user persistence contract.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    /// <summary>Creates the repository with the database context.</summary>
    /// <param name="context">EF Core database context.</param>
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Gets a user by its identifier.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching user or null when it does not exist.</returns>
    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>Gets a user by its already-normalized email.</summary>
    /// <param name="email">Email normalized with trim and lowercase.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching user or null when it does not exist.</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>Gets a page of users applying search, sorting and pagination.</summary>
    /// <param name="filter">Normalized filter with search, sort, order and paging values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged users matching the filter.</returns>
    public async Task<PagedResult<User>> GetPagedAsync(UserFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = _context.Users.AsNoTracking();

        if (filter.Search is not null)
        {
            query = query.Where(u => u.Name.Contains(filter.Search) || u.Email.Contains(filter.Search));
        }

        query = filter.Sort switch
        {
            "email" => filter.Order == "desc"
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "createdat" => filter.Order == "desc"
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            _ => filter.Order == "desc"
                ? query.OrderByDescending(u => u.Name)
                : query.OrderBy(u => u.Name)
        };

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<User>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalItems = totalItems
        };
    }

    /// <summary>Counts the tasks assigned to a user.</summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Number of tasks assigned to the user.</returns>
    public async Task<int> CountTasksByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.CountAsync(t => t.UserId == userId, cancellationToken);
    }

    /// <summary>Adds a new user.</summary>
    /// <param name="user">User to persist.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Updates an existing user.</summary>
    /// <param name="user">User with the updated values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Deletes an existing user.</summary>
    /// <param name="user">User to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
