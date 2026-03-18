using Chickquita.Application.DTOs;
using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Settings.Queries;

/// <summary>
/// Query to retrieve settings for the current tenant.
/// </summary>
public sealed record GetTenantSettingsQuery : IRequest<Result<TenantSettingsDto>>;
