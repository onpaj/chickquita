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
        var flocksGroup = app.MapGroup("/api/flocks")
            .WithTags("Flocks")
            .RequireAuthorization();

        var coopsGroup = app.MapGroup("/api/coops")
            .WithTags("Flocks")
            .RequireAuthorization();

        // GET /api/flocks - Get all flocks (optionally filtered by coopId)
        // Also available as GET /api/coops/{coopId}/flocks
        flocksGroup.MapGet("", GetFlocks)
            .WithName("GetFlocks")
            .WithOpenApi()
            .Produces<List<FlockDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // GET /api/coops/{coopId}/flocks - Get all flocks for a specific coop
        coopsGroup.MapGet("/{coopId:guid}/flocks", GetFlocksByCoop)
            .WithName("GetFlocksByCoop")
            .WithOpenApi()
            .Produces<List<FlockDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/coops/{coopId}/flocks - Create a new flock under a specific coop
        coopsGroup.MapPost("/{coopId:guid}/flocks", CreateFlock)
            .WithName("CreateFlock")
            .WithOpenApi()
            .Produces<FlockDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // GET /api/flocks/{id} - Get a specific flock by ID
        flocksGroup.MapGet("/{id:guid}", GetFlockById)
            .WithName("GetFlockById")
            .WithOpenApi()
            .Produces<FlockDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // PUT /api/flocks/{id} - Update a flock
        flocksGroup.MapPut("/{id:guid}", UpdateFlock)
            .WithName("UpdateFlock")
            .WithOpenApi()
            .Produces<FlockDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/flocks/{id}/archive - Archive a flock
        flocksGroup.MapPost("/{id:guid}/archive", ArchiveFlock)
            .WithName("ArchiveFlock")
            .WithOpenApi()
            .Produces<FlockDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/flocks/{id}/mature-chicks - Mature chicks into hens/roosters
        flocksGroup.MapPost("/{id:guid}/mature-chicks", MatureChicks)
            .WithName("MatureChicks")
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

    private static async Task<IResult> GetFlocksByCoop(
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
        [FromRoute] Guid coopId,
        [FromBody] CreateFlockCommand command,
        [FromServices] IMediator mediator)
    {
        // Ensure the CoopId from the route is used
        command.CoopId = coopId;

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

    private static async Task<IResult> MatureChicks(
        [FromRoute] Guid id,
        [FromBody] MatureChicksCommand command,
        [FromServices] IMediator mediator)
    {
        command.FlockId = id;
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
