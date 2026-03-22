namespace Chickquita.Application.DTOs;

/// <summary>
/// Data transfer object for tenant settings.
/// </summary>
public record TenantSettingsDto(bool SingleCoopMode, string Currency);
