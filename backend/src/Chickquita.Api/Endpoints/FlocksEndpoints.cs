using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Flocks.Commands;
using Chickquita.Application.Features.Flocks.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

public static class FlocksEndpoints
{
    public static void MapFlocksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/flocks")
            .WithTags("Flocks")
            .RequireAuthorization();

        group.MapGet("", GetFlocks)
            .WithName("GetFlocks")
            .WithOpenApi()
            .Produces<List<FlockDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetFlockById)
            .WithName("GetFlockById")
            .WithOpenApi()
            .Produces<FlockDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateFlock)
            .WithName("CreateFlock")
            .WithOpenApi()
            .Produces<FlockDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateFlock)
            .WithName("UpdateFlock")
            .WithOpenApi()
            .Produces<FlockDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/archive", ArchiveFlock)
            .WithName("ArchiveFlock")
            .WithOpenApi()
            .Produces<FlockDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetFlocks(
        [FromServices] IMediator mediator,
        [FromQuery] Guid? coopId = null,
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
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var query = new GetFlockByIdQuery
        {
            FlockId = id
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

    private static async Task<IResult> CreateFlock(
        [FromBody] CreateFlockCommand command,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                "Error.Validation" => Results.BadRequest(new { error = result.Error }),
                "Error.NotFound" => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Created($"/api/flocks/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdateFlock(
        [FromRoute] Guid id,
        [FromBody] UpdateFlockCommand command,
        [FromServices] IMediator mediator)
    {
        // Ensure the ID from the route matches the command
        if (id != command.FlockId)
        {
            return Results.BadRequest(new { error = new { message = "Route ID and command ID do not match" } });
        }

        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                "Error.Validation" => Results.BadRequest(new { error = result.Error }),
                "Error.NotFound" => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ArchiveFlock(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new ArchiveFlockCommand { FlockId = id };
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                "Error.Validation" => Results.BadRequest(new { error = result.Error }),
                "Error.NotFound" => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Ok(result.Value);
    }
}
