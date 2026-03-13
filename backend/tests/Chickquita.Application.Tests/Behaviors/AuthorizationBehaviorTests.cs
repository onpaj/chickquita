using Chickquita.Application.Behaviors;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using FluentAssertions;
using MediatR;
using Moq;

namespace Chickquita.Application.Tests.Behaviors;

/// <summary>
/// Unit tests for AuthorizationBehavior.
/// Verifies that the pipeline behavior enforces authentication and tenant resolution
/// for requests implementing IAuthorizedRequest, and passes through requests that don't.
/// </summary>
public class AuthorizationBehaviorTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AuthorizationBehavior<AuthorizedRequest, Result<string>> _behavior;

    public AuthorizationBehaviorTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _behavior = new AuthorizationBehavior<AuthorizedRequest, Result<string>>(_currentUserServiceMock.Object);
    }

    // A request that opts in to authorization
    private record AuthorizedRequest : IRequest<Result<string>>, IAuthorizedRequest;

    // A request that does NOT opt in to authorization
    private record UnauthorizedRequest : IRequest<Result<string>>;

    [Fact]
    public async Task Handle_AuthorizedRequest_WhenAuthenticated_ShouldCallNext()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.TenantId).Returns(Guid.NewGuid());

        var nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();
        nextMock.Setup(n => n()).ReturnsAsync(Result<string>.Success("ok"));

        // Act
        var result = await _behavior.Handle(new AuthorizedRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
        nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task Handle_AuthorizedRequest_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();

        // Act
        var result = await _behavior.Handle(new AuthorizedRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Contain("not authenticated");
        nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_AuthorizedRequest_WhenAuthenticatedButNoTenant_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(s => s.TenantId).Returns((Guid?)null);

        var nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();

        // Act
        var result = await _behavior.Handle(new AuthorizedRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Contain("Tenant");
        nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Handle_NonAuthorizedRequest_ShouldBypassAuthAndCallNext()
    {
        // Arrange: service returns not-authenticated — but it should never be consulted
        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var behavior = new AuthorizationBehavior<UnauthorizedRequest, Result<string>>(
            _currentUserServiceMock.Object);

        var nextMock = new Mock<RequestHandlerDelegate<Result<string>>>();
        nextMock.Setup(n => n()).ReturnsAsync(Result<string>.Success("bypassed"));

        // Act
        var result = await behavior.Handle(new UnauthorizedRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("bypassed");
        nextMock.Verify(n => n(), Times.Once);
        _currentUserServiceMock.Verify(s => s.IsAuthenticated, Times.Never);
    }

    [Fact]
    public async Task Handle_AuthorizedRequest_NonResultResponse_WhenNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange: TResponse is plain string (not Result<T>), so WrapFailure should throw
        var behavior = new AuthorizationBehavior<AuthorizedRequest, string>(
            _currentUserServiceMock.Object);

        _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(false);

        var nextMock = new Mock<RequestHandlerDelegate<string>>();

        // Act
        Func<Task> act = async () =>
            await behavior.Handle(new AuthorizedRequest(), nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not authenticated*");

        nextMock.Verify(n => n(), Times.Never);
    }
}
