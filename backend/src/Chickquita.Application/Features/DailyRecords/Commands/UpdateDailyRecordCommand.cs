using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.DailyRecords.Commands;

/// <summary>
/// Command to update an existing daily record for egg production.
/// Only allows updates on the same day the record was created (same-day edit restriction).
/// </summary>
public sealed record UpdateDailyRecordCommand : IRequest<Result<DailyRecordDto>>
{
    /// <summary>
    /// Gets or sets the ID of the daily record to update.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the number of eggs collected.
    /// </summary>
    public int EggCount { get; init; }

    /// <summary>
    /// Gets or sets optional notes about the daily collection.
    /// </summary>
    public string? Notes { get; init; }
}
