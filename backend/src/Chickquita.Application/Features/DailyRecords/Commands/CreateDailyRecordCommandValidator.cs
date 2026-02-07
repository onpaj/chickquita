using FluentValidation;

namespace Chickquita.Application.Features.DailyRecords.Commands;

/// <summary>
/// Validator for CreateDailyRecordCommand to ensure all input fields meet requirements.
/// </summary>
public sealed class CreateDailyRecordCommandValidator : AbstractValidator<CreateDailyRecordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDailyRecordCommandValidator"/> class.
    /// </summary>
    public CreateDailyRecordCommandValidator()
    {
        RuleFor(x => x.FlockId)
            .NotEmpty()
            .WithMessage("Flock ID is required.");

        RuleFor(x => x.RecordDate)
            .NotEmpty()
            .WithMessage("Record date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Record date cannot be in the future.");

        RuleFor(x => x.EggCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Egg count cannot be negative.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
