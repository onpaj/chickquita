using FluentValidation;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Validator for UpdateFlockCommand to ensure all input fields meet requirements.
/// </summary>
public sealed class UpdateFlockCommandValidator : AbstractValidator<UpdateFlockCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFlockCommandValidator"/> class.
    /// </summary>
    public UpdateFlockCommandValidator()
    {
        RuleFor(x => x.FlockId)
            .NotEmpty()
            .WithMessage("Flock ID is required.");

        RuleFor(x => x.Identifier)
            .NotEmpty()
            .WithMessage("Flock identifier is required.")
            .MaximumLength(50)
            .WithMessage("Flock identifier must not exceed 50 characters.");

        RuleFor(x => x.HatchDate)
            .NotEmpty()
            .WithMessage("Hatch date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Hatch date cannot be in the future.");
    }
}
