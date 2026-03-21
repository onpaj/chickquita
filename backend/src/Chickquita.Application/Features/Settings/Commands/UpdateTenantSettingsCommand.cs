using Chickquita.Domain.Common;
using MediatR;

namespace Chickquita.Application.Features.Settings.Commands;

/// <summary>
/// Command to update settings for the current tenant.
/// </summary>
public sealed record UpdateTenantSettingsCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// Gets or sets whether to enable single-coop mode.
    /// </summary>
    public bool SingleCoopMode { get; init; }

    /// <summary>
    /// Gets or sets the ISO 4217 currency code (e.g. "CZK", "EUR", "USD").
    /// Defaults to "CZK" if not provided.
    /// </summary>
    public string? Currency { get; init; }
}
