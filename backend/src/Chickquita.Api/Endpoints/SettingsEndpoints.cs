using Chickquita.Api.Extensions;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Settings.Commands;
using Chickquita.Application.Features.Settings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings")
            .WithTags("Settings")
            .RequireAuthorization();

        group.MapGet("", GetTenantSettings)
            .WithName("GetTenantSettings")
            .WithOpenApi(op =>
            {
                op.Summary = "Get tenant settings";
                op.Description = "Returns the current settings for the authenticated tenant.";
                return op;
            })
            .Produces<TenantSettingsDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("", UpdateTenantSettings)
            .WithName("UpdateTenantSettings")
            .WithOpenApi(op =>
            {
                op.Summary = "Update tenant settings";
                op.Description = "Updates the settings for the authenticated tenant.";
                return op;
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetTenantSettings(
        [FromServices] IMediator mediator)
    {
        var query = new GetTenantSettingsQuery();
        var result = await mediator.Send(query);

        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateTenantSettings(
        [FromBody] UpdateTenantSettingsRequest request,
        [FromServices] IMediator mediator)
    {
        var command = new UpdateTenantSettingsCommand { SingleCoopMode = request.SingleCoopMode };
        var result = await mediator.Send(command);

        return result.ToHttpResult(_ => Results.NoContent());
    }
}

/// <summary>
/// Request body for updating tenant settings.
/// </summary>
public sealed record UpdateTenantSettingsRequest(bool SingleCoopMode);
