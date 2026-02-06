using FluentValidation;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Validator for DeleteCoopCommand to ensure required fields are provided.
/// </summary>
public sealed class DeleteCoopCommandValidator : AbstractValidator<DeleteCoopCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteCoopCommandValidator"/> class.
    /// </summary>
    public DeleteCoopCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Coop ID is required.");
    }
}
