using FluentValidation;

namespace Chickquita.Application.Features.Users.Commands;

/// <summary>
/// Validator for SyncUserCommand to ensure required fields are provided.
/// </summary>
public sealed class SyncUserCommandValidator : AbstractValidator<SyncUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyncUserCommandValidator"/> class.
    /// </summary>
    public SyncUserCommandValidator()
    {
        RuleFor(x => x.ClerkUserId)
            .NotEmpty()
            .WithMessage("Clerk user ID is required.")
            .MaximumLength(255)
            .WithMessage("Clerk user ID must not exceed 255 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters.");
    }
}
