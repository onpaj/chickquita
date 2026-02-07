using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.DailyRecords.Commands;

/// <summary>
/// Command to create a new daily record for egg production.
/// </summary>
public sealed record CreateDailyRecordCommand : IRequest<Result<DailyRecordDto>>
{
    /// <summary>
    /// Gets or sets the ID of the flock this daily record belongs to.
    /// </summary>
    public Guid FlockId { get; init; }

    /// <summary>
    /// Gets or sets the date of the record.
    /// </summary>
    public DateTime RecordDate { get; init; }

    /// <summary>
    /// Gets or sets the number of eggs collected.
    /// </summary>
    public int EggCount { get; init; }

    /// <summary>
    /// Gets or sets optional notes about the daily collection.
    /// </summary>
    public string? Notes { get; init; }
}
