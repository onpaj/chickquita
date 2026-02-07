using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.DailyRecords.Commands;

/// <summary>
/// Command to delete a daily record.
/// </summary>
public sealed record DeleteDailyRecordCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// Gets or sets the ID of the daily record to delete.
    /// </summary>
    public Guid Id { get; init; }
}
