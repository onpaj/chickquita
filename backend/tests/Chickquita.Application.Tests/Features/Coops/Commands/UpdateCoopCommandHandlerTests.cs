using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Coops.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Coops.Commands;

/// <summary>
/// Unit tests for UpdateCoopCommandHandler.
/// Tests cover happy path and not found scenarios.
/// </summary>
public class UpdateCoopCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICoopRepository> _mockCoopRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UpdateCoopCommandHandler>> _mockLogger;
    private readonly UpdateCoopCommandHandler _handler;

    public UpdateCoopCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockCoopRepository = _fixture.Freeze<Mock<ICoopRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<UpdateCoopCommandHandler>>>();

        _handler = new UpdateCoopCommandHandler(
            _mockCoopRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateCoopSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Old Name", "Old Location");

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = "Updated Coop",
            Location = "Updated Location"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);
        _mockCoopRepository.Setup(x => x.UpdateAsync(It.IsAny<Coop>()))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(coopId))
            .ReturnsAsync(2);

        var expectedDto = new CoopDto
        {
            Id = coopId,
            TenantId = tenantId,
            Name = command.Name,
            Location = command.Location,
            IsActive = true,
            FlocksCount = 2
        };

        _mockMapper.Setup(x => x.Map<CoopDto>(It.IsAny<Coop>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Location.Should().Be(command.Location);
        result.Value.FlocksCount.Should().Be(2);

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockCoopRepository.Verify(x => x.ExistsByNameAsync(command.Name), Times.Once);
        _mockCoopRepository.Verify(x => x.UpdateAsync(It.IsAny<Coop>()), Times.Once);
        _mockCoopRepository.Verify(x => x.GetFlocksCountAsync(coopId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSameName_ShouldNotCheckForDuplicates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop Name", "Old Location");

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = "Coop Name", // Same name
            Location = "New Location"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.UpdateAsync(It.IsAny<Coop>()))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(coopId))
            .ReturnsAsync(0);

        var expectedDto = new CoopDto
        {
            Id = coopId,
            TenantId = tenantId,
            Name = command.Name,
            Location = command.Location,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<CoopDto>(It.IsAny<Coop>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockCoopRepository.Verify(x => x.ExistsByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullLocation_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Coop Name", "Old Location");

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = "Updated Name",
            Location = null
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);
        _mockCoopRepository.Setup(x => x.UpdateAsync(It.IsAny<Coop>()))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.GetFlocksCountAsync(coopId))
            .ReturnsAsync(0);

        var expectedDto = new CoopDto
        {
            Id = coopId,
            TenantId = tenantId,
            Name = command.Name,
            Location = null,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<CoopDto>(It.IsAny<Coop>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Location.Should().BeNull();
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Handle_WhenCoopNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = "Updated Coop",
            Location = "Updated Location"
        };

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
        _mockCoopRepository.Verify(x => x.UpdateAsync(It.IsAny<Coop>()), Times.Never);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new UpdateCoopCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Coop",
            Location = "Updated Location"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new UpdateCoopCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Coop",
            Location = "Updated Location"
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

        _mockCoopRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Duplicate Name Tests

    [Fact]
    public async Task Handle_WhenNewNameAlreadyExists_ShouldReturnConflictError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Old Name", "Old Location");

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = "Existing Coop Name", // This name already exists
            Location = "Updated Location"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(true); // Name already exists

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Conflict");
        result.Error.Message.Should().Be("A coop with this name already exists");

        _mockCoopRepository.Verify(x => x.UpdateAsync(It.IsAny<Coop>()), Times.Never);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyOrWhitespaceName_ShouldReturnValidationError(string invalidName)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Old Name", "Old Location");

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = invalidName,
            Location = "Updated Location"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("name cannot be empty");

        _mockCoopRepository.Verify(x => x.UpdateAsync(It.IsAny<Coop>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNameExceeding100Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Old Name", "Old Location");
        var longName = new string('A', 101);

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = longName,
            Location = "Updated Location"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot exceed 100 characters");
    }

    [Fact]
    public async Task Handle_WithLocationExceeding200Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Old Name", "Old Location");
        var longLocation = new string('B', 201);

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = "Valid Name",
            Location = longLocation
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot exceed 200 characters");
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task Handle_WhenCoopBelongsToAnotherTenant_ShouldReturnNotFound()
    {
        // Arrange
        var currentUserTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();

        // Coop belongs to a different tenant
        var otherTenantCoop = Coop.Create(otherTenantId, "Other Tenant Coop", "Other Location");

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = "Attempted Update",
            Location = "New Location"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(currentUserTenantId);

        // Repository returns null because of RLS - coop is not visible to current tenant
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync((Coop?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Coop not found");

        // Verify that update was never attempted
        _mockCoopRepository.Verify(x => x.UpdateAsync(It.IsAny<Coop>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var existingCoop = Coop.Create(tenantId, "Old Name", "Old Location");

        var command = new UpdateCoopCommand
        {
            Id = coopId,
            Name = "Updated Coop",
            Location = "Updated Location"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(existingCoop);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);
        _mockCoopRepository.Setup(x => x.UpdateAsync(It.IsAny<Coop>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to update coop");
    }

    #endregion
}
