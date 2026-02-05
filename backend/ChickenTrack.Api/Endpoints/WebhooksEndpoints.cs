using ChickenTrack.Application.Features.Users.Commands;
using ChickenTrack.Application.Interfaces;
using MediatR;

namespace ChickenTrack.Api.Endpoints;

/// <summary>
/// Defines webhook endpoints for external service integrations.
/// </summary>
public static class WebhooksEndpoints
{
    /// <summary>
    /// Maps webhook endpoints to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapWebhooksEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/webhooks")
            .WithTags("Webhooks");

        // Clerk webhook endpoint
        group.MapPost("/clerk", async (HttpRequest request, IClerkWebhookValidator validator, IMediator mediator) =>
            {
                // Read request body
                using var reader = new StreamReader(request.Body);
                var payload = await reader.ReadToEndAsync();

                // Extract headers for signature validation
                var headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

                // Validate webhook signature
                var validationResult = validator.ValidateWebhook(payload, headers);

                if (!validationResult.IsSuccess)
                {
                    return Results.Unauthorized();
                }

                var webhookDto = validationResult.Value;

                // Handle user.created event
                if (webhookDto.Type == "user.created")
                {
                    // Extract primary email
                    var primaryEmail = webhookDto.Data.EmailAddresses
                        .FirstOrDefault(e => e.Id == webhookDto.Data.PrimaryEmailAddressId)
                        ?.EmailAddress ?? string.Empty;

                    // Dispatch SyncUserCommand
                    var syncCommand = new SyncUserCommand
                    {
                        ClerkUserId = webhookDto.Data.Id,
                        Email = primaryEmail
                    };

                    await mediator.Send(syncCommand);
                }

                return Results.Ok();
            })
            .WithName("ClerkWebhook")
            .Produces(200)
            .Produces(401);

        return app;
    }
}
