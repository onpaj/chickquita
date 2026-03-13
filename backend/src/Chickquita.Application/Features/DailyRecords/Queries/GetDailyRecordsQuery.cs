using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;
using Chickquita.Application.Interfaces;

namespace Chickquita.Application.Features.DailyRecords.Queries;

/// <summary>
/// Query to get daily records with optional filtering by flock and date range.
/// </summary>
public sealed record GetDailyRecordsQuery : IRequest<Result<List<DailyRecordDto>>>, IAuthorizedRequest
{
    /// <summary>
    /// Gets or sets the optional ID of the flock to filter daily records.
    /// If null, returns all daily records for the current tenant.
    /// </summary>
    public Guid? FlockId { get; init; }

    /// <summary>
    /// Gets or sets the optional start date for filtering records (inclusive).
    /// If null, no start date filter is applied.
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Gets or sets the optional end date for filtering records (inclusive).
    /// If null, no end date filter is applied.
    /// </summary>
    public DateTime? EndDate { get; init; }
}
