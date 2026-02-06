using AutoFixture;
using AutoFixture.AutoMoq;
using Chickquita.Application.Features.Coops.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Coops.Commands;

/// <summary>
/// Unit tests for DeleteCoopCommandHandler.
/// Tests cover empty coop success and coop with flocks failure scenarios.
/// </summary>
public class DeleteCoopCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICoopRepository> _mockCoopRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<DeleteCoopCommandHandler>> _mockLogger;
    private readonly DeleteCoopCommandHandler _handler;

    public DeleteCoopCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockCoopRepository = _fixture.Freeze<Mock<ICoopRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<DeleteCoopCommandHandler>>>();

        _handler = new DeleteCoopCommandHandler(
            _mockCoopRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    #region Empty Coop Success Tests

    [Fact]
    public async Task Handle_WithEmptyCoop_ShouldDeleteSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop to Delete", "Location");

        var command = new DeleteCoopCommand { Id = coopId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.HasFlocksAsync(coopId))
            .ReturnsAsync(false); // No flocks
        _mockCoopRepository.Setup(x => x.DeleteAsync(coopId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockCoopRepository.Verify(x => x.HasFlocksAsync(coopId), Times.Once);
        _mockCoopRepository.Verify(x => x.DeleteAsync(coopId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyCoopAndNoLocation_ShouldDeleteSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop to Delete");

        var command = new DeleteCoopCommand { Id = coopId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.HasFlocksAsync(coopId))
            .ReturnsAsync(false);
        _mockCoopRepository.Setup(x => x.DeleteAsync(coopId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    #endregion

    #region Coop With Flocks Fails Tests

    [Fact]
    public async Task Handle_WithCoopHavingFlocks_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop with Flocks", "Location");

        var command = new DeleteCoopCommand { Id = coopId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.HasFlocksAsync(coopId))
            .ReturnsAsync(true); // Has flocks

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation.HAS_FLOCKS");
        result.Error.Message.Should().Contain("Cannot delete coop with existing flocks");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockCoopRepository.Verify(x => x.HasFlocksAsync(coopId), Times.Once);
        _mockCoopRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Handle_WhenCoopNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();

        var command = new DeleteCoopCommand { Id = coopId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync((Coop?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Coop not found");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockCoopRepository.Verify(x => x.HasFlocksAsync(It.IsAny<Guid>()), Times.Never);
        _mockCoopRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new DeleteCoopCommand { Id = Guid.NewGuid() };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockCoopRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new DeleteCoopCommand { Id = Guid.NewGuid() };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockCoopRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop to Delete", "Location");

        var command = new DeleteCoopCommand { Id = coopId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.HasFlocksAsync(coopId))
            .ReturnsAsync(false);
        _mockCoopRepository.Setup(x => x.DeleteAsync(coopId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to delete coop");
    }

    [Fact]
    public async Task Handle_WhenHasFlocksCheckThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop to Delete", "Location");

        var command = new DeleteCoopCommand { Id = coopId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.HasFlocksAsync(coopId))
            .ThrowsAsync(new Exception("Database query failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to delete coop");

        _mockCoopRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task Handle_WhenCoopBelongsToDifferentTenant_ShouldStillDeleteIfFoundByRepository()
    {
        // Arrange
        // Note: Tenant isolation is enforced at the repository level (via EF Core global query filters and RLS)
        // The handler trusts that GetByIdAsync only returns coops belonging to the current tenant
        // If the repository returns a coop, the handler assumes it belongs to the current tenant
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop to Delete", "Location");

        var command = new DeleteCoopCommand { Id = coopId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop); // Repository returns the coop (tenant check passed)
        _mockCoopRepository.Setup(x => x.HasFlocksAsync(coopId))
            .ReturnsAsync(false);
        _mockCoopRepository.Setup(x => x.DeleteAsync(coopId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockCoopRepository.Verify(x => x.DeleteAsync(coopId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCoopBelongsToDifferentTenant_RepositoryShouldReturnNull()
    {
        // Arrange
        // This test verifies the expected behavior: repository should return null for coops
        // belonging to other tenants (enforced by EF Core global query filters and RLS)
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();

        var command = new DeleteCoopCommand { Id = coopId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync((Coop?)null); // Repository returns null (tenant isolation enforced)

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Coop not found");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockCoopRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Delete Behavior Tests

    [Fact]
    public async Task Handle_WithValidCoop_ShouldPerformHardDelete()
    {
        // Arrange
        // Note: Current implementation performs hard delete (EF Core Remove)
        // The Coop entity has IsActive property for soft delete, but DeleteAsync uses Remove()
        // This test documents the actual behavior: hard delete from database
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop to Delete", "Location");

        var command = new DeleteCoopCommand { Id = coopId };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.HasFlocksAsync(coopId))
            .ReturnsAsync(false);
        _mockCoopRepository.Setup(x => x.DeleteAsync(coopId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Verify DeleteAsync is called (which performs hard delete in repository)
        _mockCoopRepository.Verify(x => x.DeleteAsync(coopId), Times.Once);

        // Note: In the actual repository implementation (CoopRepository.cs:71-79),
        // DeleteAsync uses _context.Coops.Remove(coop), which is a hard delete.
        // If soft delete was implemented, we would expect UpdateAsync with IsActive = false instead.
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithValidCoopId_ShouldCallRepositoryMethodsInCorrectOrder()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop to Delete", "Location");

        var command = new DeleteCoopCommand { Id = coopId };

        var callSequence = new List<string>();

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop)
            .Callback(() => callSequence.Add("GetByIdAsync"));

        _mockCoopRepository.Setup(x => x.HasFlocksAsync(coopId))
            .ReturnsAsync(false)
            .Callback(() => callSequence.Add("HasFlocksAsync"));

        _mockCoopRepository.Setup(x => x.DeleteAsync(coopId))
            .Returns(Task.CompletedTask)
            .Callback(() => callSequence.Add("DeleteAsync"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        callSequence.Should().ContainInOrder("GetByIdAsync", "HasFlocksAsync", "DeleteAsync");
    }

    #endregion
}
