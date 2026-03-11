using Chickquita.Application.Behaviors;
using Chickquita.Domain.Common;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;

namespace Chickquita.Application.Tests.Behaviors;

/// <summary>
/// Unit tests for ValidationBehavior.
/// Verifies that the pipeline behavior invokes registered validators and returns
/// Result failure or throws ValidationException on validation failures.
/// </summary>
public class ValidationBehaviorTests
{
    private record TestRequest(string Value) : IRequest<string>;
    private record ResultRequest(string Value) : IRequest<Result<string>>;

    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage("Value is required.");
        }
    }

    private class ResultRequestValidator : AbstractValidator<ResultRequest>
    {
        public ResultRequestValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage("Value is required.");
        }
    }

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>(
            Enumerable.Empty<IValidator<TestRequest>>());

        var nextMock = new Mock<RequestHandlerDelegate<string>>();
        nextMock.Setup(n => n()).ReturnsAsync("ok");

        // Act
        var result = await behavior.Handle(new TestRequest("some value"), nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { new TestRequestValidator() });

        var nextMock = new Mock<RequestHandlerDelegate<string>>();
        nextMock.Setup(n => n()).ReturnsAsync("ok");

        // Act
        var result = await behavior.Handle(new TestRequest("valid value"), nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_NonResultResponse_ShouldThrowValidationException()
    {
        // Arrange: non-Result TResponse falls back to throwing
        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { new TestRequestValidator() });

        var nextMock = new Mock<RequestHandlerDelegate<string>>();

        // Act
        Func<Task> act = async () =>
            await behavior.Handle(new TestRequest(string.Empty), nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Value is required*");

        nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ResultResponse_ShouldReturnFailureResult()
    {
        // Arrange: Result<T> response returns failure instead of throwing
        var behavior = new ValidationBehavior<ResultRequest, Result<string>>(
            new[] { new ResultRequestValidator() });

        var nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();

        // Act
        var result = await behavior.Handle(new ResultRequest(string.Empty), nextMock.Object, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("Value is required");

        nextMock.Verify(n => n(), Times.Never);
    }
}
