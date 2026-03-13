using Chickquita.Domain.Common;

namespace Chickquita.Api.Extensions;

/// <summary>
/// Extension methods for mapping Result&lt;T&gt; to IResult HTTP responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result&lt;T&gt; to an IResult HTTP response.
    /// On success, calls <paramref name="onSuccess"/> if provided, otherwise returns 200 OK with the value.
    /// On failure, maps the error code to the appropriate HTTP status code.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult>? onSuccess = null)
    {
        if (result.IsSuccess)
            return onSuccess is not null ? onSuccess(result.Value) : Results.Ok(result.Value);

        return result.Error.Code switch
        {
            "Error.Unauthorized" => Results.Unauthorized(),
            "Error.NotFound"     => Results.NotFound(new { error = result.Error }),
            "Error.Forbidden"    => Results.Forbid(),
            "Error.Conflict"     => Results.Conflict(new { error = result.Error }),
            _                    => Results.BadRequest(new { error = result.Error })
        };
    }
}
