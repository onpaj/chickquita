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

        RuleFor(x => x.CollectionTime)
            .Must(value => TimeSpan.TryParseExact(value, @"hh\:mm", null, out var t) && t >= TimeSpan.Zero && t < TimeSpan.FromHours(24))
            .WithMessage("Collection time must be a valid time in HH:mm format (00:00–23:59).")
            .When(x => !string.IsNullOrWhiteSpace(x.CollectionTime));
    }
}
