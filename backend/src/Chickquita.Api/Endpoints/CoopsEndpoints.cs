using Chickquita.Api.Extensions;
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
            .WithOpenApi(op =>
            {
                op.Summary = "Get all coops";
                op.Description = "Retrieves all coops for the current tenant. Optionally include archived coops.";
                return op;
            })
            .Produces<List<CoopDto>>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetCoopById)
            .WithName("GetCoopById")
            .WithOpenApi(op =>
            {
                op.Summary = "Get coop by ID";
                op.Description = "Retrieves a specific coop by its ID.";
                return op;
            })
            .Produces<CoopDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateCoop)
            .WithName("CreateCoop")
            .WithOpenApi(op =>
            {
                op.Summary = "Create a new coop";
                op.Description = "Creates a new coop for the current tenant.";
                return op;
            })
            .Produces<CoopDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateCoop)
            .WithName("UpdateCoop")
            .WithOpenApi(op =>
            {
                op.Summary = "Update a coop";
                op.Description = "Updates the name or description of an existing coop.";
                return op;
            })
            .Produces<CoopDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteCoop)
            .WithName("DeleteCoop")
            .WithOpenApi(op =>
            {
                op.Summary = "Delete a coop";
                op.Description = "Permanently deletes the specified coop. Fails if the coop has active flocks.";
                return op;
            })
            .Produces<bool>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/archive", ArchiveCoop)
            .WithName("ArchiveCoop")
            .WithOpenApi(op =>
            {
                op.Summary = "Archive a coop";
                op.Description = "Archives the specified coop, marking it as inactive.";
                return op;
            })
            .Produces<bool>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/ensure-default", EnsureDefaultCoop)
            .WithName("EnsureDefaultCoop")
            .WithOpenApi(op =>
            {
                op.Summary = "Ensure default coop";
                op.Description = "Returns the first existing coop for the tenant. If no coops exist, creates one named 'Default' and returns it.";
                return op;
            })
            .Produces<CoopDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GetCoops(
        [FromServices] IMediator mediator,
        [FromQuery] bool includeArchived = false)
    {
        var query = new GetCoopsQuery { IncludeArchived = includeArchived };
        var result = await mediator.Send(query);

        return result.ToHttpResult();
    }

    private static async Task<IResult> GetCoopById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var query = new GetCoopByIdQuery { Id = id };
        var result = await mediator.Send(query);

        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateCoop(
        [FromBody] CreateCoopCommand command,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(command);

        return result.ToHttpResult(value => Results.Created($"/api/coops/{value.Id}", value));
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

        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteCoop(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new DeleteCoopCommand { Id = id };
        var result = await mediator.Send(command);

        return result.ToHttpResult();
    }

    private static async Task<IResult> ArchiveCoop(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new ArchiveCoopCommand { Id = id };
        var result = await mediator.Send(command);

        return result.ToHttpResult();
    }

    private static async Task<IResult> EnsureDefaultCoop(
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new EnsureDefaultCoopCommand());

        return result.ToHttpResult();
    }
}
