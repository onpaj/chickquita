using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Coops.Commands;
using Chickquita.Application.Features.Coops.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

public static class CoopsEndpoints
{
    public static void MapCoopsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/coops")
            .WithTags("Coops")
            .RequireAuthorization();

        group.MapGet("", GetCoops)
            .WithName("GetCoops")
            .WithOpenApi()
            .Produces<List<CoopDto>>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetCoopById)
            .WithName("GetCoopById")
            .WithOpenApi()
            .Produces<CoopDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateCoop)
            .WithName("CreateCoop")
            .WithOpenApi()
            .Produces<CoopDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateCoop)
            .WithName("UpdateCoop")
            .WithOpenApi()
            .Produces<CoopDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteCoop)
            .WithName("DeleteCoop")
            .WithOpenApi()
            .Produces<bool>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/archive", ArchiveCoop)
            .WithName("ArchiveCoop")
            .WithOpenApi()
            .Produces<bool>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetCoops(
        [FromQuery] bool includeArchived,
        [FromServices] IMediator mediator)
    {
        var query = new GetCoopsQuery { IncludeArchived = includeArchived };
        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetCoopById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var query = new GetCoopByIdQuery { Id = id };
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

    private static async Task<IResult> CreateCoop(
        [FromBody] CreateCoopCommand command,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                "Error.Validation" => Results.BadRequest(new { error = result.Error }),
                "Error.Conflict" => Results.Conflict(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Created($"/api/coops/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdateCoop(
        [FromRoute] Guid id,
        [FromBody] UpdateCoopCommand command,
        [FromServices] IMediator mediator)
    {
        // Ensure the ID from the route matches the command
        if (id != command.Id)
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
                "Error.Conflict" => Results.Conflict(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DeleteCoop(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new DeleteCoopCommand { Id = id };
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

    private static async Task<IResult> ArchiveCoop(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new ArchiveCoopCommand { Id = id };
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
