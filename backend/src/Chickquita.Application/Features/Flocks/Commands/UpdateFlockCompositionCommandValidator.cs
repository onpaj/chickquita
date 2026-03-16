using FluentValidation;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Validator for UpdateFlockCompositionCommand to ensure all input fields meet requirements.
/// </summary>
public sealed class UpdateFlockCompositionCommandValidator : AbstractValidator<UpdateFlockCompositionCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFlockCompositionCommandValidator"/> class.
    /// </summary>
    public UpdateFlockCompositionCommandValidator()
    {
        RuleFor(x => x.FlockId)
            .NotEmpty()
            .WithMessage("Flock ID is required.");

        RuleFor(x => x.Hens)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Hens count cannot be negative.");

        RuleFor(x => x.Roosters)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Roosters count cannot be negative.");

        RuleFor(x => x.Chicks)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Chicks count cannot be negative.");
    }
}
