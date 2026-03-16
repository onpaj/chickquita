namespace Chickquita.Domain.Common;

/// <summary>
/// Thrown when a domain entity's business rule is violated during construction or mutation.
/// Distinct from <see cref="System.ArgumentException"/>, which signals programming errors
/// (wrong argument type/contract). This exception represents expected, domain-level failures
/// (e.g., invalid identifier length, negative count) that the application layer catches and
/// converts to a <see cref="Result{T}"/> validation failure.
/// </summary>
public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string message)
        : base(message)
    {
    }
}
