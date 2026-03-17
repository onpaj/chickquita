using Chickquita.Application.DTOs;
using Chickquita.Domain.Common;
using FluentValidation;
using MediatR;

namespace Chickquita.Application.Features.FlockHistory.Commands;

/// <summary>
/// Command to update the notes field on a flock history entry.
/// </summary>
public sealed record UpdateFlockHistoryNotesCommand : IRequest<Result<FlockHistoryDto>>
{
    /// <summary>
    /// The ID of the history entry to update.
    /// </summary>
    public required Guid HistoryId { get; init; }

    /// <summary>
    /// The new notes text (can be null to clear notes).
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Validator for UpdateFlockHistoryNotesCommand.
/// </summary>
public sealed class UpdateFlockHistoryNotesCommandValidator
    : AbstractValidator<UpdateFlockHistoryNotesCommand>
{
    public UpdateFlockHistoryNotesCommandValidator()
    {
        RuleFor(x => x.HistoryId)
            .NotEmpty()
            .WithMessage("History ID is required");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes != null)
            .WithMessage("Notes cannot exceed 500 characters");
    }
}
