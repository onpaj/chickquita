using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Coops.Queries;

public record GetCoopByIdQuery : IRequest<Result<CoopDto>>
{
    public Guid Id { get; init; }
}
