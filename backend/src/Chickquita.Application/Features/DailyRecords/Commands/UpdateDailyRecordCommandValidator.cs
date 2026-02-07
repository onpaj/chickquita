using FluentValidation;

namespace Chickquita.Application.Features.DailyRecords.Commands;

/// <summary>
/// Validator for UpdateDailyRecordCommand to ensure all input fields meet requirements.
/// </summary>
public sealed class UpdateDailyRecordCommandValidator : AbstractValidator<UpdateDailyRecordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDailyRecordCommandValidator"/> class.
    /// </summary>
    public UpdateDailyRecordCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Daily record ID is required.");

        RuleFor(x => x.EggCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Egg count cannot be negative.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
