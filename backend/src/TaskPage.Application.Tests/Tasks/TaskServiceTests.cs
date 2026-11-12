using TaskPage.Application.Tests.Fakes;
using TaskPage.Application.Tasks.DTOs;
using TaskPage.Application.Tasks.Services;
using TaskPage.Domain.Entities;
using TaskPage.Domain.Enums;
using TaskPage.Domain.Exceptions;

namespace TaskPage.Application.Tests.Tasks;

/// <summary>
/// Covers the task use cases: creation, update, deletion, status changes and listing.
/// </summary>
public class TaskServiceTests : TestBase
{
    private const string ValidJson = """{"priority":"High","tags":["backend","urgent"]}""";

    /// <summary>Creates a user with the given identifier in the fake repository.</summary>
    /// <param name="repository">Target repository.</param>
    /// <param name="id">User identifier.</param>
    /// <returns>The seeded user.</returns>
    private static User SeedUser(InMemoryUserRepository repository, int id) =>
        SeedUser(repository, id, $"User {id}");

    /// <summary>Creates a user with the given identifier and name in the fake repository.</summary>
    /// <param name="repository">Target repository.</param>
    /// <param name="id">User identifier.</param>
    /// <param name="name">User name.</param>
    /// <returns>The seeded user.</returns>
    private static User SeedUser(InMemoryUserRepository repository, int id, string name)
    {
        var user = new User { Id = id, Name = name, Email = $"user{id}@example.com", CreatedAt = DateTime.UtcNow };
        repository.Seed(user);
        return user;
    }

    /// <summary>Creates a task with a valid payload and defaults to Pending.</summary>
    [Fact]
    public async Task Create_WithValidData_ReturnsMappedTask()
    {
        await RunAsync("create task returns the mapped task with Pending status", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            var service = new TaskService(tasks, users);

            var result = await service.CreateAsync(new CreateTaskRequest
            {
                Title = "  Review API integration  ",
                UserId = 1,
                AdditionalData = ValidJson
            });

            Assert.Equal(1, result.Id);
            Assert.Equal("Review API integration", result.Title);
            Assert.Equal(TaskState.Pending, result.Status);
            Assert.Equal(user.Name, result.UserName);
            Assert.Equal(ValidJson, result.AdditionalData);
        });
    }

    /// <summary>Uses the explicit status when one is provided.</summary>
    [Fact]
    public async Task Create_WithExplicitStatus_UsesProvidedStatus()
    {
        await RunAsync("create task uses the provided status", async () =>
        {
            var users = new InMemoryUserRepository();
            SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            var service = new TaskService(tasks, users);

            var result = await service.CreateAsync(new CreateTaskRequest
            {
                Title = "Task in progress",
                UserId = 1,
                Status = TaskState.InProgress
            });

            Assert.Equal(TaskState.InProgress, result.Status);
        });
    }

    /// <summary>Rejects a task assigned to a missing user.</summary>
    [Fact]
    public async Task Create_WithNonexistentUser_ThrowsNotFound()
    {
        await RunAsync("create task with nonexistent user throws not found", async () =>
        {
            var service = new TaskService(new InMemoryTaskRepository(), new InMemoryUserRepository());

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateAsync(new CreateTaskRequest { Title = "Task", UserId = 99 }));
        });
    }

    /// <summary>Stores null additional data when only whitespace is provided.</summary>
    [Fact]
    public async Task Create_WithWhitespaceAdditionalData_StoresNull()
    {
        await RunAsync("create task with whitespace additional data stores null", async () =>
        {
            var users = new InMemoryUserRepository();
            SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            var service = new TaskService(tasks, users);

            var result = await service.CreateAsync(new CreateTaskRequest
            {
                Title = "Task",
                UserId = 1,
                AdditionalData = "   "
            });

            Assert.Null(result.AdditionalData);
        });
    }

    /// <summary>Gets an existing task by its identifier.</summary>
    [Fact]
    public async Task GetById_WithExistingTask_ReturnsTask()
    {
        await RunAsync("get existing task returns the mapped task", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 5, Title = "Seeded task", Status = TaskState.Done, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            var result = await service.GetByIdAsync(5);

            Assert.Equal(5, result.Id);
            Assert.Equal("Seeded task", result.Title);
            Assert.Equal(user.Name, result.UserName);
        });
    }

    /// <summary>Throws not found for a missing task.</summary>
    [Fact]
    public async Task GetById_WithMissingTask_ThrowsNotFound()
    {
        await RunAsync("get missing task throws not found", async () =>
        {
            var service = new TaskService(new InMemoryTaskRepository(), new InMemoryUserRepository());

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999));
        });
    }

    /// <summary>Updates the title and assigned user of an existing task.</summary>
    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedTask()
    {
        await RunAsync("update task returns the updated task", async () =>
        {
            var users = new InMemoryUserRepository();
            var first = SeedUser(users, 1);
            SeedUser(users, 2);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Old title", Status = TaskState.Pending, UserId = 1, User = first, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            var result = await service.UpdateAsync(1, new UpdateTaskRequest
            {
                Title = " New title ",
                UserId = 2,
                AdditionalData = ValidJson
            });

            Assert.Equal("New title", result.Title);
            Assert.Equal(2, result.UserId);
            Assert.Equal(ValidJson, result.AdditionalData);
            Assert.NotNull(result.UpdatedAt);
        });
    }

    /// <summary>Clears additional data when null is provided on update.</summary>
    [Fact]
    public async Task Update_WithNullAdditionalData_ClearsField()
    {
        await RunAsync("update task with null additional data clears the field", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, AdditionalData = ValidJson, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            var result = await service.UpdateAsync(1, new UpdateTaskRequest
            {
                Title = "Task",
                UserId = 1,
                AdditionalData = null
            });

            Assert.Null(result.AdditionalData);
        });
    }

    /// <summary>Throws not found when updating a missing task.</summary>
    [Fact]
    public async Task Update_WithMissingTask_ThrowsNotFound()
    {
        await RunAsync("update missing task throws not found", async () =>
        {
            var users = new InMemoryUserRepository();
            SeedUser(users, 1);
            var service = new TaskService(new InMemoryTaskRepository(), users);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateAsync(1, new UpdateTaskRequest { Title = "Task", UserId = 1 }));
        });
    }

    /// <summary>Throws not found when updating a task to a missing user.</summary>
    [Fact]
    public async Task Update_WithMissingUser_ThrowsNotFound()
    {
        await RunAsync("update task with missing user throws not found", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateAsync(1, new UpdateTaskRequest { Title = "Task", UserId = 99 }));
        });
    }

    /// <summary>Deletes an existing task.</summary>
    [Fact]
    public async Task Delete_WithExistingTask_RemovesTask()
    {
        await RunAsync("delete existing task removes the task", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            await service.DeleteAsync(1);

            Assert.Empty(tasks.Tasks);
        });
    }

    /// <summary>Throws not found when deleting a missing task.</summary>
    [Fact]
    public async Task Delete_WithMissingTask_ThrowsNotFound()
    {
        await RunAsync("delete missing task throws not found", async () =>
        {
            var service = new TaskService(new InMemoryTaskRepository(), new InMemoryUserRepository());

            await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(1));
        });
    }

    /// <summary>Rejects the forbidden Pending to Done transition.</summary>
    [Fact]
    public async Task ChangeStatus_PendingToDone_ThrowsBusinessRule()
    {
        await RunAsync("Pending to Done transition throws a business rule violation", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                service.ChangeStatusAsync(1, new UpdateTaskStatusRequest { Status = TaskState.Done }));

            Assert.Contains("Pending to Done", exception.Message);
        });
    }

    /// <summary>Allows the Pending to InProgress transition.</summary>
    [Fact]
    public async Task ChangeStatus_PendingToInProgress_IsAllowed()
    {
        await RunAsync("Pending to InProgress transition is allowed", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            var result = await service.ChangeStatusAsync(1, new UpdateTaskStatusRequest { Status = TaskState.InProgress });

            Assert.Equal(TaskState.InProgress, result.Status);
        });
    }

    /// <summary>Allows the InProgress to Done transition.</summary>
    [Fact]
    public async Task ChangeStatus_InProgressToDone_IsAllowed()
    {
        await RunAsync("InProgress to Done transition is allowed", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.InProgress, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            var result = await service.ChangeStatusAsync(1, new UpdateTaskStatusRequest { Status = TaskState.Done });

            Assert.Equal(TaskState.Done, result.Status);
        });
    }

    /// <summary>Allows reopening a task from Done to InProgress.</summary>
    [Fact]
    public async Task ChangeStatus_DoneToInProgress_IsAllowed()
    {
        await RunAsync("Done to InProgress transition is allowed", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.Done, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            var result = await service.ChangeStatusAsync(1, new UpdateTaskStatusRequest { Status = TaskState.InProgress });

            Assert.Equal(TaskState.InProgress, result.Status);
        });
    }

    /// <summary>Treats changing to the same status as an idempotent operation.</summary>
    [Fact]
    public async Task ChangeStatus_ToSameStatus_IsIdempotent()
    {
        await RunAsync("changing to the same status is idempotent", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Task", Status = TaskState.InProgress, UserId = 1, User = user, CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            var result = await service.ChangeStatusAsync(1, new UpdateTaskStatusRequest { Status = TaskState.InProgress });

            Assert.Equal(TaskState.InProgress, result.Status);
        });
    }

    /// <summary>Throws not found when changing the status of a missing task.</summary>
    [Fact]
    public async Task ChangeStatus_WithMissingTask_ThrowsNotFound()
    {
        await RunAsync("change status of a missing task throws not found", async () =>
        {
            var service = new TaskService(new InMemoryTaskRepository(), new InMemoryUserRepository());

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.ChangeStatusAsync(1, new UpdateTaskStatusRequest { Status = TaskState.InProgress }));
        });
    }

    /// <summary>Lists tasks applying filters, search, sorting and pagination.</summary>
    [Fact]
    public async Task List_AppliesFiltersSearchSortingAndPagination()
    {
        await RunAsync("list tasks applies filters, search, sorting and pagination", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Alpha task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow.AddDays(-3) });
            tasks.Seed(new TaskItem { Id = 2, Title = "Beta task", Status = TaskState.Pending, UserId = 1, User = user, CreatedAt = DateTime.UtcNow.AddDays(-2) });
            tasks.Seed(new TaskItem { Id = 3, Title = "Gamma task", Status = TaskState.Done, UserId = 1, User = user, CreatedAt = DateTime.UtcNow.AddDays(-1) });
            var service = new TaskService(tasks, users);

            var result = await service.ListAsync(new TaskFilter
            {
                Status = TaskState.Pending,
                Search = "task",
                Sort = "createdat",
                Order = "desc",
                Page = 1,
                PageSize = 2
            });

            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalItems);
            Assert.Equal("Beta task", result.Items[0].Title);
            Assert.Equal("Alpha task", result.Items[1].Title);
        });
    }

    /// <summary>Normalizes the priority filter to lowercase for case-insensitive matching.</summary>
    [Fact]
    public async Task List_NormalizesPriorityFilter()
    {
        await RunAsync("list tasks normalizes the priority filter to lowercase", async () =>
        {
            var users = new InMemoryUserRepository();
            var user = SeedUser(users, 1);
            var tasks = new InMemoryTaskRepository();
            tasks.Seed(new TaskItem { Id = 1, Title = "Urgent task", Status = TaskState.Pending, UserId = 1, User = user, AdditionalData = """{"priority":"High"}""", CreatedAt = DateTime.UtcNow });
            tasks.Seed(new TaskItem { Id = 2, Title = "Low task", Status = TaskState.Pending, UserId = 1, User = user, AdditionalData = """{"priority":"Low"}""", CreatedAt = DateTime.UtcNow });
            var service = new TaskService(tasks, users);

            var result = await service.ListAsync(new TaskFilter { Priority = "HIGH" });

            Assert.Single(result.Items);
            Assert.Equal("Urgent task", result.Items[0].Title);
        });
    }
}
