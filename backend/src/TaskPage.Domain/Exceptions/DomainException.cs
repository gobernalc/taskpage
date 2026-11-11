namespace TaskPage.Domain.Exceptions;

/// <summary>
/// Base exception for domain-level failures that carry a business meaning.
/// </summary>
public class DomainException : Exception
{
    /// <summary>Creates the exception with a business-readable message.</summary>
    /// <param name="message">Message that explains the failure.</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>Creates the exception wrapping an inner cause.</summary>
    /// <param name="message">Message that explains the failure.</param>
    /// <param name="innerException">Original exception that caused the failure.</param>
    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
