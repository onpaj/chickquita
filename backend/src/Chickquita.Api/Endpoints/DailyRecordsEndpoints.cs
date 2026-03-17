using Chickquita.Api.Extensions;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.DailyRecords.Commands;
using Chickquita.Application.Features.DailyRecords.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

public static class DailyRecordsEndpoints
{
    public static void MapDailyRecordsEndpoints(this IEndpointRouteBuilder app)
    {
        var dailyRecordsGroup = app.MapGroup("/api/daily-records")
            .WithTags("DailyRecords")
            .RequireAuthorization();

        var flocksGroup = app.MapGroup("/api/flocks")
            .WithTags("DailyRecords")
            .RequireAuthorization();

        // GET /api/daily-records - Get all daily records (optionally filtered by flockId and date range)
        // Also available as GET /api/flocks/{flockId}/daily-records
        dailyRecordsGroup.MapGet("", GetDailyRecords)
            .WithName("GetDailyRecords")
            .WithOpenApi(op =>
            {
                op.Summary = "Get all daily records";
                op.Description = "Retrieves daily egg production records for the current tenant. Optionally filter by flock ID and/or date range.";
                return op;
            })
            .Produces<List<DailyRecordDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // GET /api/flocks/{flockId}/daily-records - Get all daily records for a specific flock
        flocksGroup.MapGet("/{flockId:guid}/daily-records", GetDailyRecordsByFlock)
            .WithName("GetDailyRecordsByFlock")
            .WithOpenApi(op =>
            {
                op.Summary = "Get daily records by flock";
                op.Description = "Retrieves all daily egg production records for the specified flock, optionally filtered by date range.";
                return op;
            })
            .Produces<List<DailyRecordDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/flocks/{flockId}/daily-records - Create a new daily record for a specific flock
        flocksGroup.MapPost("/{flockId:guid}/daily-records", CreateDailyRecord)
            .WithName("CreateDailyRecord")
            .WithOpenApi(op =>
            {
                op.Summary = "Create a daily record";
                op.Description = "Records a new daily egg collection for the specified flock.";
                return op;
            })
            .Produces<DailyRecordDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // PUT /api/daily-records/{id} - Update a daily record
        dailyRecordsGroup.MapPut("/{id:guid}", UpdateDailyRecord)
            .WithName("UpdateDailyRecord")
            .WithOpenApi(op =>
            {
                op.Summary = "Update a daily record";
                op.Description = "Updates an existing daily egg production record.";
                return op;
            })
            .Produces<DailyRecordDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/daily-records/{id} - Delete a daily record
        dailyRecordsGroup.MapDelete("/{id:guid}", DeleteDailyRecord)
            .WithName("DeleteDailyRecord")
            .WithOpenApi(op =>
            {
                op.Summary = "Delete a daily record";
                op.Description = "Permanently deletes the specified daily egg production record.";
                return op;
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetDailyRecords(
        [FromServices] IMediator mediator,
        [FromQuery] Guid? flockId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetDailyRecordsQuery
        {
            FlockId = flockId,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await mediator.Send(query);

        return result.ToHttpResult();
    }

    private static async Task<IResult> GetDailyRecordsByFlock(
        [FromRoute] Guid flockId,
        [FromServices] IMediator mediator,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetDailyRecordsQuery
        {
            FlockId = flockId,
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await mediator.Send(query);

        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateDailyRecord(
        [FromRoute] Guid flockId,
        [FromBody] CreateDailyRecordCommand command,
        [FromServices] IMediator mediator)
    {
        // Ensure the FlockId from the route is used
        command = command with { FlockId = flockId };

        var result = await mediator.Send(command);

        return result.ToHttpResult(value => Results.Created($"/api/daily-records/{value.Id}", value));
    }

    private static async Task<IResult> UpdateDailyRecord(
        [FromRoute] Guid id,
        [FromBody] UpdateDailyRecordCommand command,
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

    private static async Task<IResult> DeleteDailyRecord(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new DeleteDailyRecordCommand { Id = id };
        var result = await mediator.Send(command);

        return result.ToHttpResult(_ => Results.NoContent());
    }
}
