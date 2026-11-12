using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TaskPage.Api.Controllers;
using TaskPage.Api.Tests.Fakes;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Tasks.DTOs;
using TaskPage.Application.Tasks.Services;
using TaskPage.Application.Tasks.Validators;
using TaskPage.Application.Users.Services;
using TaskPage.Application.Users.Validators;
using TaskPage.Domain.Entities;
using TaskPage.Domain.Enums;

namespace TaskPage.Api.Tests.Controllers;

/// <summary>
/// Covers the task controller contract: status codes, validation and response shapes.
/// </summary>
public class TasksControllerTests : TestBase
{
    /// <summary>Builds a task controller wired to fresh in-memory repositories.</summary>
    /// <param name="tasks">Task repository fake used by the controller.</param>
    /// <param name="users">User repository fake used by the controller.</param>
    /// <returns>The configured controller.</returns>
    private static TasksController CreateController(InMemoryTaskRepository tasks, InMemoryUserRepository users) =>
        new(
            new TaskService(tasks, users),
            new CreateTaskRequestValidator(),
            new UpdateTaskRequestValidator(),
            new UpdateTaskStatusRequestValidator(),
            NullLogger<TasksController>.Instance);

    /// <summary>Seeds a user for task assignment.</summary>
    /// <param name="users">User repository.</param>
    /// <returns>The seeded user.</returns>
    private static User SeedUser(InMemoryUserRepository users)
    {
        var user = new User { Id = 1, Name = "Ana", Email = "ana@example.com", CreatedAt = DateTime.UtcNow };
        users.Seed(user);
        return user;
    }

    /// <summary>Returns a 201 with the Location header and the created task.</summary>
    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        await RunAsync("create task returns 201 Created with the Location header", async () =>
        {
            var users = new InMemoryUserRepository();
            SeedUser(users);
            var tasks = new InMemoryTaskRepository();
            var controller = CreateController(tasks, users);

            var result = await controller.CreateAsync(new CreateTaskRequest
            {
                Title = "Review API integration",
                UserId = 1
            }, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(TasksController.GetByIdAsync), created.ActionName);
            Assert.Equal(1, created.RouteValues?["id"]);
            var response = Assert.IsType<TaskResponse>(created.Value);
            Assert.Equal(TaskState.Pending, response.Status);
        });
    }

    /// <summary>Rejects invalid input before reaching the service.</summary>
    [Fact]
    public async Task Create_WithInvalidRequest_ThrowsValidationException()
    {
        await RunAsync("create task with invalid input throws a validation exception", async () =>
        {
            var users = new InMemoryUserRepository();
            SeedUser(users);
            var controller = CreateController(new InMemoryTaskRepository(), users);

            await Assert.ThrowsAsync<ValidationException>(() =>
                controller.CreateAsync(new CreateTaskRequest
                {
                    Title = "",
                    UserId = 1
                }, CancellationToken.None));
        });
    }

    /// <summary>Returns 200 with the requested task.</summary>
    [Fact]
    public async Task GetById_ReturnsOkWithResponse()
    {
        await RunAsync("get task returns 200 OK with the response", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var controller = CreateController(tasks, users);

            var result = await controller.GetByIdAsync(1, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TaskResponse>(ok.Value);
            Assert.Equal(1, response.Id);
        });
    }

    /// <summary>Returns 200 with a paged result.</summary>
    [Fact]
    public async Task List_ReturnsOkWithPagedResult()
    {
        await RunAsync("list tasks returns 200 OK with a paged result", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var controller = CreateController(tasks, users);

            var result = await controller.ListAsync(new TaskFilter(), CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var page = Assert.IsType<PagedResult<TaskResponse>>(ok.Value);
            Assert.Single(page.Items);
        });
    }

    /// <summary>Returns 200 with the updated task.</summary>
    [Fact]
    public async Task Update_WithValidRequest_ReturnsOk()
    {
        await RunAsync("update task returns 200 OK with the updated task", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var controller = CreateController(tasks, users);

            var result = await controller.UpdateAsync(1, new UpdateTaskRequest
            {
                Title = "Updated task",
                UserId = 1
            }, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TaskResponse>(ok.Value);
            Assert.Equal("Updated task", response.Title);
        });
    }

    /// <summary>Rejects an invalid update before reaching the service.</summary>
    [Fact]
    public async Task Update_WithInvalidRequest_ThrowsValidationException()
    {
        await RunAsync("update task with invalid input throws a validation exception", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var controller = CreateController(tasks, users);

            await Assert.ThrowsAsync<ValidationException>(() =>
                controller.UpdateAsync(1, new UpdateTaskRequest
                {
                    Title = "",
                    UserId = -1
                }, CancellationToken.None));
        });
    }

    /// <summary>Returns 200 with the updated status.</summary>
    [Fact]
    public async Task ChangeStatus_WithValidRequest_ReturnsOk()
    {
        await RunAsync("change status returns 200 OK with the updated task", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var controller = CreateController(tasks, users);

            var result = await controller.ChangeStatusAsync(1, new UpdateTaskStatusRequest
            {
                Status = TaskState.InProgress
            }, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TaskResponse>(ok.Value);
            Assert.Equal(TaskState.InProgress, response.Status);
        });
    }

    /// <summary>Rejects an invalid status before reaching the service.</summary>
    [Fact]
    public async Task ChangeStatus_WithInvalidStatus_ThrowsValidationException()
    {
        await RunAsync("change status with an invalid status throws a validation exception", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var controller = CreateController(tasks, users);

            await Assert.ThrowsAsync<ValidationException>(() =>
                controller.ChangeStatusAsync(1, new UpdateTaskStatusRequest
                {
                    Status = (TaskState)99
                }, CancellationToken.None));
        });
    }

    /// <summary>Returns 204 No Content after deleting the task.</summary>
    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        await RunAsync("delete task returns 204 No Content", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var controller = CreateController(tasks, users);

            var result = await controller.DeleteAsync(1, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(tasks.Tasks);
        });
    }
}
