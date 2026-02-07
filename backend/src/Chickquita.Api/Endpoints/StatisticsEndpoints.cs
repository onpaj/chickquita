using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Statistics.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

/// <summary>
/// Endpoints for retrieving aggregated statistics.
/// </summary>
public static class StatisticsEndpoints
{
    /// <summary>
    /// Maps statistics-related endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/statistics")
            .WithTags("Statistics")
            .RequireAuthorization();

        group.MapGet("/dashboard", GetDashboardStats)
            .WithName("GetDashboardStats")
            .WithOpenApi()
            .Produces<DashboardStatsDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Gets aggregated dashboard statistics for the current tenant.
    /// Returns total coops, active flocks, total hens, and total animals.
    /// </summary>
    /// <param name="mediator">The mediator instance for dispatching the query.</param>
    /// <returns>Dashboard statistics DTO containing aggregated metrics.</returns>
    private static async Task<IResult> GetDashboardStats(
        [FromServices] IMediator mediator)
    {
        var query = new GetDashboardStatsQuery();
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
}
