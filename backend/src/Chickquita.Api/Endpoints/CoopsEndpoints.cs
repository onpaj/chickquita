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

        group.MapPost("", CreateCoop)
            .WithName("CreateCoop")
            .WithOpenApi()
            .Produces<CoopDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetCoops(
        [FromServices] IMediator mediator)
    {
        var query = new GetCoopsQuery();
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
}
