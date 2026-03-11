using Chickquita.Application.Interfaces;
using MediatR;

namespace Chickquita.Api.Endpoints;

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
        // TODO: Task 4 - Update to handle organization.created event using SyncOrgCommand
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

                // TODO: Task 4 - Handle organization.created event with SyncOrgCommand
                return Results.Ok();
            })
            .WithName("ClerkWebhook")
            .Produces(200)
            .Produces(401);

        return app;
    }
}
