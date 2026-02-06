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
/// Unit tests for CreateCoopCommandHandler.
/// Tests cover happy path, duplicate name validation, and empty name validation.
/// </summary>
public class CreateCoopCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICoopRepository> _mockCoopRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CreateCoopCommandHandler>> _mockLogger;
    private readonly CreateCoopCommandHandler _handler;

    public CreateCoopCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockCoopRepository = _fixture.Freeze<Mock<ICoopRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<CreateCoopCommandHandler>>>();

        _handler = new CreateCoopCommandHandler(
            _mockCoopRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateCoopSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = "North Field"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);

        var createdCoop = Coop.Create(tenantId, command.Name, command.Location);
        _mockCoopRepository.Setup(x => x.AddAsync(It.IsAny<Coop>()))
            .ReturnsAsync(createdCoop);

        var expectedDto = new CoopDto
        {
            Id = createdCoop.Id,
            TenantId = tenantId,
            Name = command.Name,
            Location = command.Location,
            IsActive = true,
            FlocksCount = 0
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
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.IsActive.Should().BeTrue();
        result.Value.FlocksCount.Should().Be(0);

        _mockCoopRepository.Verify(x => x.ExistsByNameAsync(command.Name), Times.Once);
        _mockCoopRepository.Verify(x => x.AddAsync(It.IsAny<Coop>()), Times.Once);
        _mockMapper.Verify(x => x.Map<CoopDto>(It.IsAny<Coop>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidDataWithoutLocation_ShouldCreateCoopSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = null
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);

        var createdCoop = Coop.Create(tenantId, command.Name, command.Location);
        _mockCoopRepository.Setup(x => x.AddAsync(It.IsAny<Coop>()))
            .ReturnsAsync(createdCoop);

        var expectedDto = new CoopDto
        {
            Id = createdCoop.Id,
            TenantId = tenantId,
            Name = command.Name,
            Location = null,
            IsActive = true,
            FlocksCount = 0
        };

        _mockMapper.Setup(x => x.Map<CoopDto>(It.IsAny<Coop>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Location.Should().BeNull();
        result.Value.FlocksCount.Should().Be(0);
    }

    #endregion

    #region Duplicate Name Tests

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnConflictError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = "North Field"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(true); // Simulate duplicate name

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Conflict");
        result.Error.Message.Should().Be("A coop with this name already exists");

        _mockCoopRepository.Verify(x => x.ExistsByNameAsync(command.Name), Times.Once);
        _mockCoopRepository.Verify(x => x.AddAsync(It.IsAny<Coop>()), Times.Never);
        _mockMapper.Verify(x => x.Map<CoopDto>(It.IsAny<Coop>()), Times.Never);
    }

    #endregion

    #region Empty Name Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyOrWhitespaceName_ShouldReturnValidationError(string invalidName)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCoopCommand
        {
            Name = invalidName,
            Location = "North Field"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("name cannot be empty");

        _mockCoopRepository.Verify(x => x.AddAsync(It.IsAny<Coop>()), Times.Never);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = "North Field"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockCoopRepository.Verify(x => x.ExistsByNameAsync(It.IsAny<string>()), Times.Never);
        _mockCoopRepository.Verify(x => x.AddAsync(It.IsAny<Coop>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = "North Field"
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

        _mockCoopRepository.Verify(x => x.ExistsByNameAsync(It.IsAny<string>()), Times.Never);
        _mockCoopRepository.Verify(x => x.AddAsync(It.IsAny<Coop>()), Times.Never);
    }

    #endregion

    #region Validation Edge Cases

    [Fact]
    public async Task Handle_WithNameExceeding100Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var longName = new string('A', 101);
        var command = new CreateCoopCommand
        {
            Name = longName,
            Location = "North Field"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

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
        var longLocation = new string('B', 201);
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = longLocation
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot exceed 200 characters");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateCoopCommand
        {
            Name = "Main Coop",
            Location = "North Field"
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.ExistsByNameAsync(command.Name))
            .ReturnsAsync(false);
        _mockCoopRepository.Setup(x => x.AddAsync(It.IsAny<Coop>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to create coop");
    }

    #endregion
}
