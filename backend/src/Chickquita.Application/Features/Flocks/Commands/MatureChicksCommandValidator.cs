using FluentValidation;

namespace Chickquita.Application.Features.Flocks.Commands;

/// <summary>
/// Validator for the MatureChicksCommand.
/// </summary>
public sealed class MatureChicksCommandValidator : AbstractValidator<MatureChicksCommand>
{
    public MatureChicksCommandValidator()
    {
        RuleFor(x => x.FlockId)
            .NotEmpty()
            .WithMessage("Flock ID is required.");

        RuleFor(x => x.ChicksToMature)
            .GreaterThan(0)
            .WithMessage("ChicksToMature must be greater than 0.");

        RuleFor(x => x.Hens)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Hens count cannot be negative.");

        RuleFor(x => x.Roosters)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Roosters count cannot be negative.");

        RuleFor(x => x)
            .Must(cmd => cmd.Hens + cmd.Roosters == cmd.ChicksToMature)
            .WithMessage("The sum of hens and roosters must equal ChicksToMature.");
    }
}
