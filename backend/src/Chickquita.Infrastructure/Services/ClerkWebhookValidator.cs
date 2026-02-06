using System.Net;
using System.Text.Json;
using Chickquita.Application.DTOs;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using Microsoft.Extensions.Configuration;
using Svix;

namespace Chickquita.Infrastructure.Services;

/// <summary>
/// Validates Clerk webhook signatures using Svix to ensure authenticity.
/// </summary>
public sealed class ClerkWebhookValidator : IClerkWebhookValidator
{
    private readonly string _webhookSecret;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClerkWebhookValidator"/> class.
    /// </summary>
    /// <param name="configuration">Application configuration containing Clerk webhook secret</param>
    public ClerkWebhookValidator(IConfiguration configuration)
    {
        _webhookSecret = configuration["Clerk:WebhookSecret"]
            ?? throw new InvalidOperationException("Clerk webhook secret is not configured");
    }

    /// <inheritdoc />
    public Result<ClerkWebhookDto> ValidateWebhook(string payload, IDictionary<string, string> headers)
    {
        try
        {
            // Create Svix webhook instance with the secret
            var webhook = new Webhook(_webhookSecret);

            // Extract Svix headers (Clerk uses Svix for webhook signatures)
            if (!headers.TryGetValue("svix-id", out var svixId) ||
                !headers.TryGetValue("svix-timestamp", out var svixTimestamp) ||
                !headers.TryGetValue("svix-signature", out var svixSignature))
            {
                return Result<ClerkWebhookDto>.Failure(
                    new Error("MISSING_HEADERS", "Required Svix headers are missing"));
            }

            // Create WebHeaderCollection for Svix validation
            var webhookHeaders = new WebHeaderCollection
            {
                { "svix-id", svixId },
                { "svix-timestamp", svixTimestamp },
                { "svix-signature", svixSignature }
            };

            // Verify the webhook signature using Svix
            // This will throw an exception if signature is invalid
            webhook.Verify(payload, webhookHeaders);

            // Deserialize the verified payload to ClerkWebhookDto
            var webhookDto = JsonSerializer.Deserialize<ClerkWebhookDto>(
                payload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (webhookDto == null)
            {
                return Result<ClerkWebhookDto>.Failure(
                    new Error("INVALID_PAYLOAD", "Failed to deserialize webhook payload"));
            }

            return Result<ClerkWebhookDto>.Success(webhookDto);
        }
        catch (Exception ex) when (ex.GetType().Name == "WebhookVerificationException")
        {
            // Signature validation failed - webhook is not authentic
            return Result<ClerkWebhookDto>.Failure(
                new Error("INVALID_SIGNATURE", $"Webhook signature validation failed: {ex.Message}"));
        }
        catch (JsonException ex)
        {
            return Result<ClerkWebhookDto>.Failure(
                new Error("INVALID_JSON", $"Invalid JSON payload: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result<ClerkWebhookDto>.Failure(
                new Error("UNEXPECTED_ERROR", $"Unexpected error during webhook validation: {ex.Message}"));
        }
    }
}
