using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TaskPage.Api.Controllers;
using TaskPage.Api.Tests.Fakes;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Users.DTOs;
using TaskPage.Application.Users.Services;
using TaskPage.Application.Users.Validators;
using TaskPage.Domain.Entities;

namespace TaskPage.Api.Tests.Controllers;

/// <summary>
/// Covers the user controller contract: status codes, validation and response shapes.
/// </summary>
public class UsersControllerTests : TestBase
{
    /// <summary>Builds a controller wired to fresh in-memory repositories.</summary>
    /// <param name="repository">User repository fake used by the controller.</param>
    /// <returns>The configured controller.</returns>
    private static UsersController CreateController(InMemoryUserRepository repository) =>
        new(
            new UserService(repository),
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator(),
            NullLogger<UsersController>.Instance);

    /// <summary>Returns a 201 with the Location header and the created user.</summary>
    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        await RunAsync("create user returns 201 Created with the Location header", async () =>
        {
            var repository = new InMemoryUserRepository();
            var controller = CreateController(repository);

            var result = await controller.CreateAsync(new CreateUserRequest
            {
                Name = "Ana Pérez",
                Email = "ana.perez@example.com"
            }, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            // MVC strips the "Async" suffix from action names, so the Location URL must
            // target the "GetById" action generated from GetByIdAsync.
            var expectedActionName = nameof(UsersController.GetByIdAsync)[..^"Async".Length];
            Assert.Equal(expectedActionName, created.ActionName);
            Assert.Equal(1, created.RouteValues?["id"]);
            var response = Assert.IsType<UserResponse>(created.Value);
            Assert.Equal("ana.perez@example.com", response.Email);
        });
    }

    /// <summary>Rejects invalid input before reaching the service.</summary>
    [Fact]
    public async Task Create_WithInvalidRequest_ThrowsValidationException()
    {
        await RunAsync("create user with invalid input throws a validation exception", async () =>
        {
            var controller = CreateController(new InMemoryUserRepository());

            await Assert.ThrowsAsync<ValidationException>(() =>
                controller.CreateAsync(new CreateUserRequest
                {
                    Name = "",
                    Email = "invalid"
                }, CancellationToken.None));
        });
    }

    /// <summary>Returns 200 with the requested user.</summary>
    [Fact]
    public async Task GetById_ReturnsOkWithResponse()
    {
        await RunAsync("get user returns 200 OK with the response", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var controller = CreateController(repository);

            var result = await controller.GetByIdAsync(1, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<UserResponse>(ok.Value);
            Assert.Equal(1, response.Id);
        });
    }

    /// <summary>Returns 200 with a paged result.</summary>
    [Fact]
    public async Task List_ReturnsOkWithPagedResult()
    {
        await RunAsync("list users returns 200 OK with a paged result", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var controller = CreateController(repository);

            var result = await controller.ListAsync(new UserFilter(), CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var page = Assert.IsType<PagedResult<UserResponse>>(ok.Value);
            Assert.Single(page.Items);
        });
    }

    /// <summary>Returns 200 with the updated user.</summary>
    [Fact]
    public async Task Update_WithValidRequest_ReturnsOk()
    {
        await RunAsync("update user returns 200 OK with the updated user", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var controller = CreateController(repository);

            var result = await controller.UpdateAsync(1, new UpdateUserRequest
            {
                Name = "Ana Updated",
                Email = "ana@example.com"
            }, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<UserResponse>(ok.Value);
            Assert.Equal("Ana Updated", response.Name);
        });
    }

    /// <summary>Rejects an invalid update before reaching the service.</summary>
    [Fact]
    public async Task Update_WithInvalidRequest_ThrowsValidationException()
    {
        await RunAsync("update user with invalid input throws a validation exception", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var controller = CreateController(repository);

            await Assert.ThrowsAsync<ValidationException>(() =>
                controller.UpdateAsync(1, new UpdateUserRequest
                {
                    Name = "",
                    Email = "invalid"
                }, CancellationToken.None));
        });
    }

    /// <summary>Returns 204 No Content after deleting the user.</summary>
    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        await RunAsync("delete user returns 204 No Content", async () =>
        {
            var repository = new InMemoryUserRepository();
            repository.Seed(new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow });
            var controller = CreateController(repository);

            var result = await controller.DeleteAsync(1, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(repository.Users);
        });
    }
}
