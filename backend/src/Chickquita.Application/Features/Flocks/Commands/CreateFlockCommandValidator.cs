using FluentValidation;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Validator for CreateFlockCommand to ensure all input fields meet requirements.
/// </summary>
public sealed class CreateFlockCommandValidator : AbstractValidator<CreateFlockCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateFlockCommandValidator"/> class.
    /// </summary>
    public CreateFlockCommandValidator()
    {
        RuleFor(x => x.CoopId)
            .NotEmpty()
            .WithMessage("Coop ID is required.");

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

        RuleFor(x => x.InitialHens)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial hens count cannot be negative.");

        RuleFor(x => x.InitialRoosters)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial roosters count cannot be negative.");

        RuleFor(x => x.InitialChicks)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial chicks count cannot be negative.");

        RuleFor(x => x)
            .Must(x => x.InitialHens + x.InitialRoosters + x.InitialChicks > 0)
            .WithMessage("At least one animal type must have a count greater than 0.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
