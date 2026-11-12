using TaskPage.Application.Tasks.DTOs;
using TaskPage.Application.Tasks.Validators;
using TaskPage.Domain.Enums;

namespace TaskPage.Application.Tests.Tasks;

/// <summary>
/// Covers the validation rules for the task input DTOs.
/// </summary>
public class TaskRequestValidatorsTests : TestBase
{
    /// <summary>Accepts a valid create request.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithValidData_Passes()
    {
        await RunAsync("create task request with valid data passes validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = "Review API integration",
                UserId = 1,
                Status = TaskState.Pending,
                AdditionalData = """{"priority":"High"}"""
            });

            Assert.True(result.IsValid);
        });
    }

    /// <summary>Rejects an empty title.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithEmptyTitle_Fails()
    {
        await RunAsync("create task request with empty title fails validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = "",
                UserId = 1
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        });
    }

    /// <summary>Rejects a null title.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithNullTitle_Fails()
    {
        await RunAsync("create task request with null title fails validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = null!,
                UserId = 1
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        });
    }

    /// <summary>Rejects a title longer than the maximum length.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithTooLongTitle_Fails()
    {
        await RunAsync("create task request with a title over the limit fails validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = new string('a', 201),
                UserId = 1
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        });
    }

    /// <summary>Rejects a non-positive user identifier.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithInvalidUserId_Fails()
    {
        await RunAsync("create task request with a non-positive user id fails validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = "Task",
                UserId = 0
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
        });
    }

    /// <summary>Rejects a status outside the enum.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithInvalidStatus_Fails()
    {
        await RunAsync("create task request with an invalid status fails validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = "Task",
                UserId = 1,
                Status = (TaskState)99
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Status");
        });
    }

    /// <summary>Rejects malformed JSON in the additional data.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithInvalidJson_Fails()
    {
        await RunAsync("create task request with invalid JSON fails validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = "Task",
                UserId = 1,
                AdditionalData = "not-json"
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AdditionalData");
        });
    }

    /// <summary>Rejects scalar JSON values such as strings or numbers.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithScalarJson_Fails()
    {
        await RunAsync("create task request with a scalar JSON value fails validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = "Task",
                UserId = 1,
                AdditionalData = "\"just a string\""
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AdditionalData");
        });
    }

    /// <summary>Accepts a JSON array as additional data.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithJsonArray_Passes()
    {
        await RunAsync("create task request with a JSON array passes validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = "Task",
                UserId = 1,
                AdditionalData = """["backend","urgent"]"""
            });

            Assert.True(result.IsValid);
        });
    }

    /// <summary>Accepts blank additional data as optional.</summary>
    [Fact]
    public async Task CreateTaskRequest_WithBlankAdditionalData_Passes()
    {
        await RunAsync("create task request with blank additional data passes validation", async () =>
        {
            var validator = new CreateTaskRequestValidator();

            var result = await validator.ValidateAsync(new CreateTaskRequest
            {
                Title = "Task",
                UserId = 1,
                AdditionalData = "   "
            });

            Assert.True(result.IsValid);
        });
    }

    /// <summary>Accepts a valid update request.</summary>
    [Fact]
    public async Task UpdateTaskRequest_WithValidData_Passes()
    {
        await RunAsync("update task request with valid data passes validation", async () =>
        {
            var validator = new UpdateTaskRequestValidator();

            var result = await validator.ValidateAsync(new UpdateTaskRequest
            {
                Title = "Updated title",
                UserId = 1,
                AdditionalData = """{"priority":"Low"}"""
            });

            Assert.True(result.IsValid);
        });
    }

    /// <summary>Rejects an invalid update request.</summary>
    [Fact]
    public async Task UpdateTaskRequest_WithInvalidData_Fails()
    {
        await RunAsync("update task request with invalid data fails validation", async () =>
        {
            var validator = new UpdateTaskRequestValidator();

            var result = await validator.ValidateAsync(new UpdateTaskRequest
            {
                Title = "",
                UserId = -1,
                AdditionalData = "invalid"
            });

            Assert.False(result.IsValid);
        });
    }

    /// <summary>Accepts a valid status change request.</summary>
    [Fact]
    public async Task UpdateTaskStatusRequest_WithValidStatus_Passes()
    {
        await RunAsync("status change request with a valid status passes validation", async () =>
        {
            var validator = new UpdateTaskStatusRequestValidator();

            var result = await validator.ValidateAsync(new UpdateTaskStatusRequest { Status = TaskState.Done });

            Assert.True(result.IsValid);
        });
    }

    /// <summary>Rejects a status outside the enum.</summary>
    [Fact]
    public async Task UpdateTaskStatusRequest_WithInvalidStatus_Fails()
    {
        await RunAsync("status change request with an invalid status fails validation", async () =>
        {
            var validator = new UpdateTaskStatusRequestValidator();

            var result = await validator.ValidateAsync(new UpdateTaskStatusRequest { Status = (TaskState)99 });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Status");
        });
    }
}
