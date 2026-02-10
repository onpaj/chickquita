using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Flocks.Queries;

/// <summary>
/// Query to get the full history of a flock's composition changes.
/// </summary>
public sealed record GetFlockHistoryQuery : IRequest<Result<List<FlockHistoryDto>>>
{
    /// <summary>
    /// The ID of the flock to get history for.
    /// </summary>
    public required Guid FlockId { get; init; }
}

/// <summary>
/// Handler for GetFlockHistoryQuery.
/// Returns all flock history entries sorted by change date (newest first).
/// </summary>
public sealed class GetFlockHistoryQueryHandler
    : IRequestHandler<GetFlockHistoryQuery, Result<List<FlockHistoryDto>>>
{
    private readonly IFlockRepository _flockRepository;
    private readonly IMapper _mapper;

    public GetFlockHistoryQueryHandler(
        IFlockRepository flockRepository,
        IMapper mapper)
    {
        _flockRepository = flockRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<FlockHistoryDto>>> Handle(
        GetFlockHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Get the flock with its history (tenant isolation handled by RLS and global filters)
        var flock = await _flockRepository.GetByIdAsync(request.FlockId);

        if (flock == null)
        {
            return Result<List<FlockHistoryDto>>.Failure(
                Error.NotFound("Flock not found"));
        }

        // Map history entries to DTOs (already sorted by ChangeDate DESC in repository)
        var historyDtos = _mapper.Map<List<FlockHistoryDto>>(flock.History);

        return Result<List<FlockHistoryDto>>.Success(historyDtos);
    }
}
