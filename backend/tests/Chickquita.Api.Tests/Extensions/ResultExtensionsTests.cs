using Chickquita.Api.Extensions;
using Chickquita.Domain.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Chickquita.Api.Tests.Extensions;

/// <summary>
/// Unit tests for ResultExtensions.ToHttpResult().
/// Verifies that each error code maps to the correct HTTP status code.
/// </summary>
public class ResultExtensionsTests
{
    // ── Success ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToHttpResult_Success_Returns200WithValue()
    {
        var result = Result<string>.Success("hello");

        var httpResult = result.ToHttpResult();

        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public void ToHttpResult_Success_ValueIsPreserved()
    {
        var result = Result<string>.Success("hello");

        var httpResult = result.ToHttpResult();

        httpResult.Should().BeOfType<Ok<string>>()
            .Which.Value.Should().Be("hello");
    }

    [Fact]
    public void ToHttpResult_SuccessWithOnSuccessCallback_CallsCallback()
    {
        var result = Result<string>.Success("hello");
        var callbackCalled = false;

        var httpResult = result.ToHttpResult(value =>
        {
            callbackCalled = true;
            return Results.Created("/api/items/1", value);
        });

        callbackCalled.Should().BeTrue();
        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult!.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public void ToHttpResult_SuccessWithNullOnSuccessCallback_Returns200()
    {
        var result = Result<int>.Success(42);

        var httpResult = result.ToHttpResult(onSuccess: null);

        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    // ── Error.Unauthorized ────────────────────────────────────────────────────

    [Fact]
    public void ToHttpResult_UnauthorizedError_Returns401()
    {
        var result = Result<string>.Failure(Error.Unauthorized("Not authenticated"));

        var httpResult = result.ToHttpResult();

        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    // ── Error.NotFound ────────────────────────────────────────────────────────

    [Fact]
    public void ToHttpResult_NotFoundError_Returns404()
    {
        var result = Result<string>.Failure(Error.NotFound("Item not found"));

        var httpResult = result.ToHttpResult();

        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    // ── Error.Forbidden ───────────────────────────────────────────────────────

    [Fact]
    public void ToHttpResult_ForbiddenError_ReturnsForbidResult()
    {
        var result = Result<string>.Failure(Error.Forbidden("Access denied"));

        var httpResult = result.ToHttpResult();

        httpResult.Should().BeOfType<ForbidHttpResult>();
    }

    // ── Error.Conflict ────────────────────────────────────────────────────────

    [Fact]
    public void ToHttpResult_ConflictError_Returns409()
    {
        var result = Result<string>.Failure(Error.Conflict("Resource already exists"));

        var httpResult = result.ToHttpResult();

        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    // ── Error.Failure / unknown codes → BadRequest ────────────────────────────

    [Fact]
    public void ToHttpResult_FailureError_Returns400()
    {
        var result = Result<string>.Failure(Error.Failure("Something went wrong"));

        var httpResult = result.ToHttpResult();

        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void ToHttpResult_ValidationError_Returns400()
    {
        var result = Result<string>.Failure(Error.Validation("Name is required"));

        var httpResult = result.ToHttpResult();

        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void ToHttpResult_ValidationWithSubcode_Returns400()
    {
        var result = Result<string>.Failure(Error.ValidationWithCode("Name", "Name is required"));

        var httpResult = result.ToHttpResult();

        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void ToHttpResult_UnknownErrorCode_Returns400()
    {
        var error = new Error("Some.CustomCode", "Unexpected error");
        var result = Result<string>.Failure(error);

        var httpResult = result.ToHttpResult();

        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    // ── onSuccess callback is NOT called on failure ───────────────────────────

    [Fact]
    public void ToHttpResult_FailureWithOnSuccessCallback_CallbackNotInvoked()
    {
        var result = Result<string>.Failure(Error.NotFound("Not found"));
        var callbackCalled = false;

        var httpResult = result.ToHttpResult(value =>
        {
            callbackCalled = true;
            return Results.Created("/api/items/1", value);
        });

        callbackCalled.Should().BeFalse();
        var statusResult = httpResult as IStatusCodeHttpResult;
        statusResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
