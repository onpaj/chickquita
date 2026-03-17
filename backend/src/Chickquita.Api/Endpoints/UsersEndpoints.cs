using Chickquita.Api.Extensions;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chickquita.Api.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithOpenApi(op =>
            {
                op.Summary = "Get current user";
                op.Description = "Returns the profile of the currently authenticated user.";
                return op;
            })
            .Produces<UserDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetCurrentUser(
        [FromServices] IMediator mediator)
    {
        var query = new GetCurrentUserQuery();
        var result = await mediator.Send(query);

        return result.ToHttpResult();
    }
}
