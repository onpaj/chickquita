using Chickquita.Api.Extensions;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.EggSales.Commands.Create;
using Chickquita.Application.Features.EggSales.Commands.Update;
using Chickquita.Application.Features.EggSales.Commands.Delete;
using Chickquita.Application.Features.EggSales.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

public static class EggSalesEndpoints
{
    public static void MapEggSalesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/egg-sales")
            .WithTags("EggSales")
            .RequireAuthorization();

        group.MapGet("", GetEggSales)
            .WithName("GetEggSales")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get all egg sales";
                operation.Description = "Retrieves a list of egg sales for the current tenant with optional date range filtering.";
                return operation;
            })
            .Produces<List<EggSaleDto>>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetEggSaleById)
            .WithName("GetEggSaleById")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get egg sale by ID";
                operation.Description = "Retrieves a specific egg sale by its unique identifier. Only returns records belonging to the current tenant.";
                return operation;
            })
            .Produces<EggSaleDto>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", CreateEggSale)
            .WithName("CreateEggSale")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create a new egg sale";
                operation.Description = "Creates a new egg sale record for the current tenant.";
                return operation;
            })
            .Produces<EggSaleDto>(StatusCodes.Status201Created, "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:guid}", UpdateEggSale)
            .WithName("UpdateEggSale")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update an existing egg sale";
                operation.Description = "Updates an egg sale record. Only records belonging to the current tenant can be updated.";
                return operation;
            })
            .Produces<EggSaleDto>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteEggSale)
            .WithName("DeleteEggSale")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Delete an egg sale";
                operation.Description = "Permanently deletes an egg sale record. Only records belonging to the current tenant can be deleted.";
                return operation;
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetEggSales(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromServices] IMediator mediator)
    {
        var query = new GetEggSalesQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
        };

        var result = await mediator.Send(query);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetEggSaleById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var query = new GetEggSaleByIdQuery { Id = id };
        var result = await mediator.Send(query);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateEggSale(
        [FromBody] CreateEggSaleCommand command,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(command);
        return result.ToHttpResult(value => Results.Created($"/api/egg-sales/{value.Id}", value));
    }

    private static async Task<IResult> UpdateEggSale(
        [FromRoute] Guid id,
        [FromBody] UpdateEggSaleCommand command,
        [FromServices] IMediator mediator)
    {
        if (id != command.Id)
        {
            return Results.BadRequest(new { error = new { message = "Route ID and command ID do not match" } });
        }

        var result = await mediator.Send(command);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteEggSale(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new DeleteEggSaleCommand { EggSaleId = id };
        var result = await mediator.Send(command);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
