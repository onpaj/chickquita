using FluentValidation;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Validator for UpdateCoopCommand to ensure required fields are provided.
/// </summary>
public sealed class UpdateCoopCommandValidator : AbstractValidator<UpdateCoopCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCoopCommandValidator"/> class.
    /// </summary>
    public UpdateCoopCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Coop ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Coop name is required.")
            .MaximumLength(100)
            .WithMessage("Coop name must not exceed 100 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(200)
            .WithMessage("Location must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Location));
    }
}
