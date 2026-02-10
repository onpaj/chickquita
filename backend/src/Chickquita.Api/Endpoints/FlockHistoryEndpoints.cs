using Chickquita.Application.DTOs;
using Chickquita.Application.Features.FlockHistory.Commands;
using Chickquita.Application.Features.Flocks.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

public static class FlockHistoryEndpoints
{
    public static void MapFlockHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var flocksGroup = app.MapGroup("/api/flocks")
            .WithTags("Flocks")
            .RequireAuthorization();

        var historyGroup = app.MapGroup("/api/flock-history")
            .WithTags("FlockHistory")
            .RequireAuthorization();

        // GET /api/flocks/{id}/history - Get full flock change history timeline
        flocksGroup.MapGet("/{id:guid}/history", GetFlockHistory)
            .WithName("GetFlockHistory")
            .WithOpenApi()
            .Produces<List<FlockHistoryDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // PATCH /api/flock-history/{id}/notes - Update notes on history record
        historyGroup.MapPatch("/{id:guid}/notes", UpdateFlockHistoryNotes)
            .WithName("UpdateFlockHistoryNotes")
            .WithOpenApi()
            .Produces<FlockHistoryDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetFlockHistory(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var query = new GetFlockHistoryQuery { FlockId = id };
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                "Error.NotFound" or "Flock.NotFound" => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateFlockHistoryNotes(
        [FromRoute] Guid id,
        [FromBody] UpdateFlockHistoryNotesRequest request,
        [FromServices] IMediator mediator)
    {
        var command = new UpdateFlockHistoryNotesCommand
        {
            HistoryId = id,
            Notes = request.Notes
        };

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                "Error.NotFound" or "FlockHistory.NotFound" => Results.NotFound(new { error = result.Error }),
                "Error.Validation" or "FlockHistory.ValidationError" => Results.BadRequest(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Ok(result.Value);
    }
}

/// <summary>
/// Request model for updating flock history notes.
/// </summary>
public sealed record UpdateFlockHistoryNotesRequest
{
    /// <summary>
    /// The new notes text (can be null to clear notes).
    /// </summary>
    public string? Notes { get; init; }
}
