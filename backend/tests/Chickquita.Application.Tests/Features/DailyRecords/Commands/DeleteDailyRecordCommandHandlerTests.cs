using AutoFixture;
using AutoFixture.AutoMoq;
using Chickquita.Application.Features.DailyRecords.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.DailyRecords.Commands;

/// <summary>
/// Unit tests for DeleteDailyRecordCommandHandler.
/// Tests cover successful deletion, validation scenarios, and tenant isolation.
/// </summary>
public class DeleteDailyRecordCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IDailyRecordRepository> _mockDailyRecordRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<DeleteDailyRecordCommandHandler>> _mockLogger;
    private readonly DeleteDailyRecordCommandHandler _handler;

    public DeleteDailyRecordCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockDailyRecordRepository = _fixture.Freeze<Mock<IDailyRecordRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<DeleteDailyRecordCommandHandler>>>();

        _handler = new DeleteDailyRecordCommandHandler(
            _mockDailyRecordRepository.Object,
            _mockCurrentUserService.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidDailyRecordId_ShouldDeleteDailyRecordSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;

        var command = new DeleteDailyRecordCommand
        {
            Id = dailyRecordId
        };

        var existingDailyRecord = DailyRecord.Create(
            tenantId,
            flockId,
            recordDate,
            25,
            "Daily record to be deleted");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);
        _mockDailyRecordRepository.Setup(x => x.DeleteAsync(dailyRecordId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(dailyRecordId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.DeleteAsync(dailyRecordId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidDailyRecordIdAndNoNotes_ShouldDeleteSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;

        var command = new DeleteDailyRecordCommand
        {
            Id = dailyRecordId
        };

        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, recordDate, 10, null);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);
        _mockDailyRecordRepository.Setup(x => x.DeleteAsync(dailyRecordId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockDailyRecordRepository.Verify(x => x.DeleteAsync(dailyRecordId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOldDailyRecord_ShouldDeleteSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var oldRecordDate = DateTime.UtcNow.Date.AddDays(-30);

        var command = new DeleteDailyRecordCommand
        {
            Id = dailyRecordId
        };

        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, oldRecordDate, 15, "Old record");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);
        _mockDailyRecordRepository.Setup(x => x.DeleteAsync(dailyRecordId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _mockDailyRecordRepository.Verify(x => x.DeleteAsync(dailyRecordId), Times.Once);
    }

    #endregion

    #region Daily Record Not Found Tests

    [Fact]
    public async Task Handle_WhenDailyRecordDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();

        var command = new DeleteDailyRecordCommand
        {
            Id = dailyRecordId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync((DailyRecord?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Daily record not found");

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(dailyRecordId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDailyRecordBelongsToDifferentTenant_ShouldReturnNotFoundError()
    {
        // Arrange
        var currentTenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;

        var command = new DeleteDailyRecordCommand
        {
            Id = dailyRecordId
        };

        // The repository should return null for records from different tenants due to RLS/global filters
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(currentTenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync((DailyRecord?)null); // Simulating tenant isolation

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Daily record not found");

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(dailyRecordId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Authentication and Tenant Isolation Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new DeleteDailyRecordCommand
        {
            Id = Guid.NewGuid()
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(It.IsAny<Guid>()), Times.Never);
        _mockDailyRecordRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new DeleteDailyRecordCommand
        {
            Id = Guid.NewGuid()
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockDailyRecordRepository.Verify(x => x.GetByIdWithoutNavigationAsync(It.IsAny<Guid>()), Times.Never);
        _mockDailyRecordRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var recordDate = DateTime.UtcNow.Date;

        var command = new DeleteDailyRecordCommand
        {
            Id = dailyRecordId
        };

        var existingDailyRecord = DailyRecord.Create(tenantId, flockId, recordDate, 20, "Test");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ReturnsAsync(existingDailyRecord);
        _mockDailyRecordRepository.Setup(x => x.DeleteAsync(dailyRecordId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to delete daily record");
    }

    [Fact]
    public async Task Handle_WhenGetByIdThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dailyRecordId = Guid.NewGuid();

        var command = new DeleteDailyRecordCommand
        {
            Id = dailyRecordId
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetByIdWithoutNavigationAsync(dailyRecordId))
            .ThrowsAsync(new Exception("Database query failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to delete daily record");

        _mockDailyRecordRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion
}
