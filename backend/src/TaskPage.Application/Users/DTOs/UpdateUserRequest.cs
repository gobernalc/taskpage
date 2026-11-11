namespace TaskPage.Application.Users.DTOs;

/// <summary>
/// Input data to replace the values of an existing user.
/// </summary>
public class UpdateUserRequest
{
    /// <summary>Full name of the collaborator.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Email address, normalized by the service when saved.</summary>
    public string Email { get; set; } = string.Empty;
}
