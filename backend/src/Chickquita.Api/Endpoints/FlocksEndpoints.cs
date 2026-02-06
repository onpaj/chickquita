using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Flocks.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

public static class FlocksEndpoints
{
    public static void MapFlocksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/coops/{coopId:guid}/flocks")
            .WithTags("Flocks")
            .RequireAuthorization();

        group.MapGet("", GetFlocks)
            .WithName("GetFlocks")
            .WithOpenApi()
            .Produces<List<FlockDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{flockId:guid}", GetFlockById)
            .WithName("GetFlockById")
            .WithOpenApi()
            .Produces<FlockDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetFlocks(
        [FromRoute] Guid coopId,
        [FromServices] IMediator mediator,
        [FromQuery] bool includeInactive = false)
    {
        var query = new GetFlocksQuery
        {
            CoopId = coopId,
            IncludeInactive = includeInactive
        };
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                "Error.NotFound" => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetFlockById(
        [FromRoute] Guid coopId,
        [FromRoute] Guid flockId,
        [FromServices] IMediator mediator)
    {
        var query = new GetFlockByIdQuery
        {
            FlockId = flockId
        };
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                "Error.NotFound" => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Ok(result.Value);
    }
}
