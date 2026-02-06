using FluentValidation;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Validator for ArchiveCoopCommand to ensure required fields are provided.
/// </summary>
public sealed class ArchiveCoopCommandValidator : AbstractValidator<ArchiveCoopCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArchiveCoopCommandValidator"/> class.
    /// </summary>
    public ArchiveCoopCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Coop ID is required.");
    }
}
