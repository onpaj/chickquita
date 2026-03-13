using Chickquita.Application.Behaviors;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using FluentAssertions;
using MediatR;
using Moq;

namespace Chickquita.Application.Tests.Behaviors;

/// <summary>
/// Unit tests for AuthorizationBehavior.
/// Verifies that the pipeline behavior enforces authentication and tenant checks,
/// and skips them for IAnonymousRequest implementations.
/// </summary>
public class AuthorizationBehaviorTests
{
    private record AuthenticatedRequest : IRequest<Result<string>>;
    private record AnonymousRequest : IRequest<Result<string>>, IAnonymousRequest;
    private record NonResultRequest : IRequest<string>;

    private readonly Mock<ICurrentUserService> _mockCurrentUserService;

    public AuthorizationBehaviorTests()
    {
        _mockCurrentUserService = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        var behavior = new AuthorizationBehavior<AuthenticatedRequest, Result<string>>(
            _mockCurrentUserService.Object);

        var nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();

        // Act
        var result = await behavior.Handle(new AuthenticatedRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");
        nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        var behavior = new AuthorizationBehavior<AuthenticatedRequest, Result<string>>(
            _mockCurrentUserService.Object);

        var nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();

        // Act
        var result = await behavior.Handle(new AuthenticatedRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");
        nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatedWithTenant_ShouldCallNext()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        var behavior = new AuthorizationBehavior<AuthenticatedRequest, Result<string>>(
            _mockCurrentUserService.Object);

        var nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();
        nextMock.Setup(n => n()).ReturnsAsync(Result<string>.Success("ok"));

        // Act
        var result = await behavior.Handle(new AuthenticatedRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
        nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequestIsAnonymous_ShouldSkipChecksAndCallNext()
    {
        // Arrange — do NOT set up IsAuthenticated or TenantId; behavior should not call them
        var behavior = new AuthorizationBehavior<AnonymousRequest, Result<string>>(
            _mockCurrentUserService.Object);

        var nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();
        nextMock.Setup(n => n()).ReturnsAsync(Result<string>.Success("skipped"));

        // Act
        var result = await behavior.Handle(new AnonymousRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("skipped");
        nextMock.Verify(n => n(), Times.Once);
        _mockCurrentUserService.Verify(x => x.IsAuthenticated, Times.Never);
        _mockCurrentUserService.Verify(x => x.TenantId, Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticatedAndResponseIsNotResult_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        var behavior = new AuthorizationBehavior<NonResultRequest, string>(
            _mockCurrentUserService.Object);

        var nextMock = new Mock<RequestHandlerDelegate<string>>();

        // Act
        Func<Task> act = async () =>
            await behavior.Handle(new NonResultRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated");

        nextMock.Verify(n => n(), Times.Never);
    }
}
