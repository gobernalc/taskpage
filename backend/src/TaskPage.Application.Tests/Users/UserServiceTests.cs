using TaskPage.Application.Tests.Fakes;
using TaskPage.Application.Users.DTOs;
using TaskPage.Application.Users.Services;
using TaskPage.Domain.Entities;
using TaskPage.Domain.Exceptions;

namespace TaskPage.Application.Tests.Users;

/// <summary>
/// Covers the user use cases: creation, update, deletion, lookup and listing.
/// </summary>
public class UserServiceTests : TestBase
{
    /// <summary>Creates a valid user and returns its mapped response.</summary>
    [Fact]
    public async Task Create_WithValidData_ReturnsMappedUser()
    {
        await RunAsync("create user returns the mapped user", async () =>
        {
            var repository = new InMemoryUserRepository();
            var service = new UserService(repository);

            var result = await service.CreateAsync(new CreateUserRequest
            {
                Name = "Ana Pérez",
                Email = "ana.perez@example.com"
            });

            Assert.Equal(1, result.Id);
            Assert.Equal("Ana Pérez", result.Name);
            Assert.Equal("ana.perez@example.com", result.Email);
            Assert.Single(repository.Users);
        });
    }

    /// <summary>Normalizes the email by trimming whitespace and lowercasing it.</summary>
    [Fact]
    public async Task Create_NormalizesEmailAndName()
    {
        await RunAsync("create user normalizes email and trims the name", async () =>
        {
            var repository = new InMemoryUserRepository();
            var service = new UserService(repository);

            var result = await service.CreateAsync(new CreateUserRequest
            {
                Name = "  Ana Pérez  ",
                Email = "  ANA.PEREZ@EXAMPLE.COM  "
            });

            Assert.Equal("Ana Pérez", result.Name);
            Assert.Equal("ana.perez@example.com", result.Email);
        });
    }

    /// <summary>Preserves special characters in the name.</summary>
    [Fact]
    public async Task Create_WithSpecialCharacters_PreservesName()
    {
        await RunAsync("create user preserves special characters in the name", async () =>
        {
            var repository = new InMemoryUserRepository();
            var service = new UserService(repository);

            var result = await service.CreateAsync(new CreateUserRequest
            {
                Name = "José María O'Neil — QA",
                Email = "jose.maria@example.com"
            });

            Assert.Equal("José María O'Neil — QA", result.Name);
        });
    }

    /// <summary>Rejects a duplicated email with a conflict.</summary>
    [Fact]
    public async Task Create_WithDuplicateEmail_ThrowsConflict()
    {
        await RunAsync("create user with duplicated email throws conflict", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var service = new UserService(repository);

            var exception = await Assert.ThrowsAsync<ConflictException>(() =>
                service.CreateAsync(new CreateUserRequest
                {
                    Name = "Other Ana",
                    Email = "ANA@example.com"
                }));

            Assert.Contains("already registered", exception.Message);
        });
    }

    /// <summary>Gets an existing user by its identifier.</summary>
    [Fact]
    public async Task GetById_WithExistingUser_ReturnsUser()
    {
        await RunAsync("get existing user returns the mapped user", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 7, Name = "Luis", Email = "luis@example.com", CreatedAt = DateTime.UtcNow });
            var service = new UserService(repository);

            var result = await service.GetByIdAsync(7);

            Assert.Equal(7, result.Id);
            Assert.Equal("Luis", result.Name);
        });
    }

    /// <summary>Throws not found for a missing user.</summary>
    [Fact]
    public async Task GetById_WithMissingUser_ThrowsNotFound()
    {
        await RunAsync("get missing user throws not found", async () =>
        {
            var service = new UserService(new InMemoryUserRepository());

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999));
        });
    }

    /// <summary>Updates an existing user and normalizes the new email.</summary>
    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedUser()
    {
        await RunAsync("update user returns the updated user", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var service = new UserService(repository);

            var result = await service.UpdateAsync(1, new UpdateUserRequest
            {
                Name = "Ana María",
                Email = " ANA.MARIA@EXAMPLE.COM "
            });

            Assert.Equal("Ana María", result.Name);
            Assert.Equal("ana.maria@example.com", result.Email);
            Assert.NotNull(result.UpdatedAt);
        });
    }

    /// <summary>Rejects an update that would duplicate another user's email.</summary>
    [Fact]
    public async Task Update_ToDuplicateEmail_ThrowsConflict()
    {
        await RunAsync("update user to a duplicated email throws conflict", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            repository.Seed(new User { Id = 2, Name = "Luis", Email = "luis@example.com", CreatedAt = DateTime.UtcNow });
            var service = new UserService(repository);

            await Assert.ThrowsAsync<ConflictException>(() =>
                service.UpdateAsync(1, new UpdateUserRequest
                {
                    Name = "Ana",
                    Email = "LUIS@example.com"
                }));
        });
    }

    /// <summary>Allows keeping the same email on update.</summary>
    [Fact]
    public async Task Update_ToSameEmail_AllowsUpdate()
    {
        await RunAsync("update user to the same email is allowed", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var service = new UserService(repository);

            var result = await service.UpdateAsync(1, new UpdateUserRequest
            {
                Name = "Ana Updated",
                Email = "ana@example.com"
            });

            Assert.Equal("Ana Updated", result.Name);
        });
    }

    /// <summary>Throws not found when updating a missing user.</summary>
    [Fact]
    public async Task Update_WithMissingUser_ThrowsNotFound()
    {
        await RunAsync("update missing user throws not found", async () =>
        {
            var service = new UserService(new InMemoryUserRepository());

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateAsync(1, new UpdateUserRequest { Name = "Ana", Email = "ana@example.com" }));
        });
    }

    /// <summary>Deletes a user with no associated tasks.</summary>
    [Fact]
    public async Task Delete_WithoutTasks_RemovesUser()
    {
        await RunAsync("delete user without tasks removes the user", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var service = new UserService(repository);

            await service.DeleteAsync(1);

            Assert.Empty(repository.Users);
        });
    }

    /// <summary>Rejects the deletion of a user with associated tasks.</summary>
    [Fact]
    public async Task Delete_WithAssociatedTasks_ThrowsConflict()
    {
        await RunAsync("delete user with associated tasks throws conflict", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            repository.SetTaskCount(1, 2);
            var service = new UserService(repository);

            var exception = await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(1));

            Assert.Contains("associated tasks", exception.Message);
            Assert.Single(repository.Users);
        });
    }

    /// <summary>Throws not found when deleting a missing user.</summary>
    [Fact]
    public async Task Delete_WithMissingUser_ThrowsNotFound()
    {
        await RunAsync("delete missing user throws not found", async () =>
        {
            var service = new UserService(new InMemoryUserRepository());

            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(1));
        });
    }

    /// <summary>Lists users applying search, sorting and pagination and maps every item.</summary>
    [Fact]
    public async Task List_AppliesSearchSortingAndPagination()
    {
        await RunAsync("list users applies search, sorting and pagination", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow.AddDays(-2) });
            repository.Seed(new User { Id = 2, Name = "Luis", Email = "luis@example.com", CreatedAt = DateTime.UtcNow.AddDays(-1) });
            repository.Seed(new User { Id = 3, Name = "Marcos", Email = "marcos@example.com", CreatedAt = DateTime.UtcNow });
            var service = new UserService(repository);

            var result = await service.ListAsync(new UserFilter
            {
                Search = "example",
                Sort = "name",
                Order = "asc",
                Page = 1,
                PageSize = 2
            });

            Assert.Equal(2, result.Items.Count);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal("Ana", result.Items[0].Name);
            Assert.Equal("Luis", result.Items[1].Name);
            Assert.All(result.Items, item => Assert.IsType<UserResponse>(item));
        });
    }

    /// <summary>Clamps invalid pagination values before listing.</summary>
    [Fact]
    public async Task List_ClampsInvalidPagingValues()
    {
        await RunAsync("list users clamps invalid paging values", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var service = new UserService(repository);

            var result = await service.ListAsync(new UserFilter { Page = 0, PageSize = 500 });

            Assert.Equal(1, result.Page);
            Assert.Equal(100, result.PageSize);
        });
    }
}
