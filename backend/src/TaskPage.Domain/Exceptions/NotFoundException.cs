namespace TaskPage.Domain.Exceptions;

/// <summary>
/// Signals that an aggregate or entity does not exist (maps to HTTP 404).
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>Creates the exception with the missing resource description.</summary>
    /// <param name="message">Message that identifies the missing resource.</param>
    public NotFoundException(string message) : base(message)
    {
    }
}
