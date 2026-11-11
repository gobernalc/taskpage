namespace TaskPage.Domain.Exceptions;

/// <summary>
/// Signals a violation of a business rule (maps to HTTP 400).
/// </summary>
public class BusinessRuleException : DomainException
{
    /// <summary>Creates the exception with the violated rule description.</summary>
    /// <param name="message">Message that describes the violated business rule.</param>
    public BusinessRuleException(string message) : base(message)
    {
    }
}
