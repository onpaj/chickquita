using Chickquita.Application.Features.Users.Commands;
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

                // Handle organization.created and organization.updated events
                if (webhookDto.Type == "organization.created" || webhookDto.Type == "organization.updated")
                {
                    var syncCommand = new SyncOrgCommand
                    {
                        ClerkOrgId = webhookDto.Data.Id,
                        Name = webhookDto.Data.Name ?? webhookDto.Data.Id
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
