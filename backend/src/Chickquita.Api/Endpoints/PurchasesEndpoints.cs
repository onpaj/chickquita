using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Purchases.Commands.Create;
using Chickquita.Application.Features.Purchases.Commands.Update;
using Chickquita.Application.Features.Purchases.Commands.Delete;
using Chickquita.Application.Features.Purchases.Queries;
using Chickquita.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

public static class PurchasesEndpoints
{
    public static void MapPurchasesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/purchases")
            .WithTags("Purchases")
            .RequireAuthorization();

        group.MapGet("", GetPurchases)
            .WithName("GetPurchases")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get all purchases";
                operation.Description = "Retrieves a list of purchases for the current tenant with optional filtering by date range, type, and flock.";
                return operation;
            })
            .Produces<List<PurchaseDto>>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetPurchaseById)
            .WithName("GetPurchaseById")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get purchase by ID";
                operation.Description = "Retrieves a specific purchase by its unique identifier. Only returns purchases belonging to the current tenant.";
                return operation;
            })
            .Produces<PurchaseDto>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/names", GetPurchaseNames)
            .WithName("GetPurchaseNames")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get purchase names for autocomplete";
                operation.Description = "Retrieves a list of distinct purchase names for autocomplete functionality. Supports optional search query and limit parameters.";
                return operation;
            })
            .Produces<List<string>>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("", CreatePurchase)
            .WithName("CreatePurchase")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create a new purchase";
                operation.Description = "Creates a new purchase record for the current tenant. Validates that the referenced coop (if provided) belongs to the tenant.";
                return operation;
            })
            .Produces<PurchaseDto>(StatusCodes.Status201Created, "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdatePurchase)
            .WithName("UpdatePurchase")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update an existing purchase";
                operation.Description = "Updates a purchase record. Only purchases belonging to the current tenant can be updated. The ID in the URL must match the ID in the request body.";
                return operation;
            })
            .Produces<PurchaseDto>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeletePurchase)
            .WithName("DeletePurchase")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Delete a purchase";
                operation.Description = "Permanently deletes a purchase record. Only purchases belonging to the current tenant can be deleted.";
                return operation;
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetPurchases(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] PurchaseType? type,
        [FromQuery] Guid? flockId,
        [FromServices] IMediator mediator)
    {
        var query = new GetPurchasesQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            Type = type,
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

    private static async Task<IResult> GetPurchaseById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var query = new GetPurchaseByIdQuery { Id = id };
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

    private static async Task<IResult> GetPurchaseNames(
        [FromServices] IMediator mediator,
        [FromQuery] string? query = null,
        [FromQuery] int limit = 20)
    {
        var purchaseNamesQuery = new GetPurchaseNamesQuery
        {
            Query = query,
            Limit = limit
        };

        var result = await mediator.Send(purchaseNamesQuery);

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

    private static async Task<IResult> CreatePurchase(
        [FromBody] CreatePurchaseCommand command,
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

        return Results.Created($"/api/purchases/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdatePurchase(
        [FromRoute] Guid id,
        [FromBody] UpdatePurchaseCommand command,
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
                "Error.Forbidden" => Results.Forbid(),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DeletePurchase(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new DeletePurchaseCommand { PurchaseId = id };
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Error.Unauthorized" => Results.Unauthorized(),
                "Error.Validation" => Results.BadRequest(new { error = result.Error }),
                "Error.NotFound" => Results.NotFound(new { error = result.Error }),
                "Error.Forbidden" => Results.Forbid(),
                _ => Results.BadRequest(new { error = result.Error })
            };
        }

        return Results.NoContent();
    }
}
