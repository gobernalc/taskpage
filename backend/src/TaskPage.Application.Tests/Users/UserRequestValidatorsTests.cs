using TaskPage.Application.Users.DTOs;
using TaskPage.Application.Users.Validators;

namespace TaskPage.Application.Tests.Users;

/// <summary>
/// Covers the validation rules for the user input DTOs.
/// </summary>
public class UserRequestValidatorsTests : TestBase
{
    /// <summary>Accepts a valid create request.</summary>
    [Fact]
    public async Task CreateUserRequest_WithValidData_Passes()
    {
        await RunAsync("create user request with valid data passes validation", async () =>
        {
            var validator = new CreateUserRequestValidator();

            var result = await validator.ValidateAsync(new CreateUserRequest
            {
                Name = "Ana Pérez",
                Email = "ana.perez@example.com"
            });

            Assert.True(result.IsValid);
        });
    }

    /// <summary>Rejects an empty name.</summary>
    [Fact]
    public async Task CreateUserRequest_WithEmptyName_Fails()
    {
        await RunAsync("create user request with empty name fails validation", async () =>
        {
            var validator = new CreateUserRequestValidator();

            var result = await validator.ValidateAsync(new CreateUserRequest
            {
                Name = "",
                Email = "ana@example.com"
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        });
    }

    /// <summary>Rejects a null name.</summary>
    [Fact]
    public async Task CreateUserRequest_WithNullName_Fails()
    {
        await RunAsync("create user request with null name fails validation", async () =>
        {
            var validator = new CreateUserRequestValidator();

            var result = await validator.ValidateAsync(new CreateUserRequest
            {
                Name = null!,
                Email = "ana@example.com"
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        });
    }

    /// <summary>Rejects an invalid email address.</summary>
    [Fact]
    public async Task CreateUserRequest_WithInvalidEmail_Fails()
    {
        await RunAsync("create user request with invalid email fails validation", async () =>
        {
            var validator = new CreateUserRequestValidator();

            var result = await validator.ValidateAsync(new CreateUserRequest
            {
                Name = "Ana",
                Email = "not-an-email"
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        });
    }

    /// <summary>Rejects a name longer than the maximum length.</summary>
    [Fact]
    public async Task CreateUserRequest_WithTooLongName_Fails()
    {
        await RunAsync("create user request with a name over the limit fails validation", async () =>
        {
            var validator = new CreateUserRequestValidator();

            var result = await validator.ValidateAsync(new CreateUserRequest
            {
                Name = new string('a', 101),
                Email = "ana@example.com"
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        });
    }

    /// <summary>Rejects an email longer than the maximum length.</summary>
    [Fact]
    public async Task CreateUserRequest_WithTooLongEmail_Fails()
    {
        await RunAsync("create user request with an email over the limit fails validation", async () =>
        {
            var validator = new CreateUserRequestValidator();

            var result = await validator.ValidateAsync(new CreateUserRequest
            {
                Name = "Ana",
                Email = new string('a', 196) + "@x.co"
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        });
    }

    /// <summary>Accepts a name and email containing special characters.</summary>
    [Fact]
    public async Task CreateUserRequest_WithSpecialCharacters_Passes()
    {
        await RunAsync("create user request with special characters passes validation", async () =>
        {
            var validator = new CreateUserRequestValidator();

            var result = await validator.ValidateAsync(new CreateUserRequest
            {
                Name = "José María O'Neil — QA",
                Email = "jose.maria+tag@example.com"
            });

            Assert.True(result.IsValid);
        });
    }

    /// <summary>Accepts a valid update request.</summary>
    [Fact]
    public async Task UpdateUserRequest_WithValidData_Passes()
    {
        await RunAsync("update user request with valid data passes validation", async () =>
        {
            var validator = new UpdateUserRequestValidator();

            var result = await validator.ValidateAsync(new UpdateUserRequest
            {
                Name = "Ana Pérez",
                Email = "ana.perez@example.com"
            });

            Assert.True(result.IsValid);
        });
    }

    /// <summary>Rejects an invalid update request.</summary>
    [Fact]
    public async Task UpdateUserRequest_WithInvalidData_Fails()
    {
        await RunAsync("update user request with invalid data fails validation", async () =>
        {
            var validator = new UpdateUserRequestValidator();

            var result = await validator.ValidateAsync(new UpdateUserRequest
            {
                Name = "",
                Email = "invalid"
            });

            Assert.False(result.IsValid);
        });
    }
}
