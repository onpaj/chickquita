using MediatR;

namespace ChickenTrack.Application.Features.System;

/// <summary>
/// Simple ping query to verify MediatR is configured correctly
/// </summary>
public record PingQuery : IRequest<string>;

/// <summary>
/// Handler for the ping query
/// </summary>
public class PingQueryHandler : IRequestHandler<PingQuery, string>
{
    public Task<string> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong! MediatR is working correctly.");
    }
}
