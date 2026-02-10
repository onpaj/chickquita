using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
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

/// <summary>
/// Handler for UpdateFlockHistoryNotesCommand.
/// </summary>
public sealed class UpdateFlockHistoryNotesCommandHandler
    : IRequestHandler<UpdateFlockHistoryNotesCommand, Result<FlockHistoryDto>>
{
    private readonly IFlockHistoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateFlockHistoryNotesCommand> _validator;

    public UpdateFlockHistoryNotesCommandHandler(
        IFlockHistoryRepository repository,
        IMapper mapper,
        IValidator<UpdateFlockHistoryNotesCommand> validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<FlockHistoryDto>> Handle(
        UpdateFlockHistoryNotesCommand request,
        CancellationToken cancellationToken)
    {
        // Validate the command
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            return Result<FlockHistoryDto>.Failure(
                Error.Validation(string.Join("; ", errors)));
        }

        // Find the history entry (tenant isolation handled by RLS and global filters)
        var historyEntry = await _repository.GetByIdAsync(request.HistoryId);

        if (historyEntry == null)
        {
            return Result<FlockHistoryDto>.Failure(
                Error.NotFound("Flock history entry not found"));
        }

        // Update the notes using the domain method
        historyEntry.UpdateNotes(request.Notes);

        // Save changes
        await _repository.UpdateAsync(historyEntry);

        // Map to DTO and return
        var dto = _mapper.Map<FlockHistoryDto>(historyEntry);

        return Result<FlockHistoryDto>.Success(dto);
    }
}
