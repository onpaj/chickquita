using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;
using MediatR;

namespace Chickquita.Application.Features.Coops.Commands;

/// <summary>
/// Command that ensures the current tenant has at least one coop.
/// If no coops exist, a default coop named "Default" is created.
/// Returns the existing coop if one already exists.
/// </summary>
public sealed record EnsureDefaultCoopCommand : IRequest<Result<CoopDto>>;
