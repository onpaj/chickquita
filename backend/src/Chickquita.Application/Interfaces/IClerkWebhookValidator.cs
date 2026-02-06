using Chickquita.Domain.Common;
using Chickquita.Application.DTOs;

namespace Chickquita.Application.Interfaces;

/// <summary>
/// Interface for validating Clerk webhook signatures using Svix.
/// Ensures that incoming webhooks are authentic and haven't been tampered with.
/// </summary>
public interface IClerkWebhookValidator
{
    /// <summary>
    /// Validates a Clerk webhook signature and extracts the payload.
    /// </summary>
    /// <param name="payload">The raw webhook payload (request body as string)</param>
    /// <param name="headers">The webhook headers containing signature information</param>
    /// <returns>
    /// A Result containing the validated ClerkWebhookDto if authentic,
    /// or an Error if validation fails (tampered or invalid signature)
    /// </returns>
    Result<ClerkWebhookDto> ValidateWebhook(string payload, IDictionary<string, string> headers);
}
