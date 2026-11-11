namespace TaskPage.Domain.Exceptions;

/// <summary>
/// Signals a state conflict such as a duplicate email or a user with tasks
/// (maps to HTTP 409).
/// </summary>
public class ConflictException : DomainException
{
    /// <summary>Creates the exception with the conflict description.</summary>
    /// <param name="message">Message that describes the conflict.</param>
    public ConflictException(string message) : base(message)
    {
    }
}
