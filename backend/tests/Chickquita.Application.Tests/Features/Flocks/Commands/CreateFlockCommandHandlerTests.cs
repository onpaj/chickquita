using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Flocks.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.Flocks.Commands;

/// <summary>
/// Unit tests for CreateFlockCommandHandler.
/// Tests cover all validation scenarios, tenant isolation, and initial history creation.
/// </summary>
public class CreateFlockCommandHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFlockRepository> _mockFlockRepository;
    private readonly Mock<ICoopRepository> _mockCoopRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CreateFlockCommandHandler>> _mockLogger;
    private readonly CreateFlockCommandHandler _handler;

    public CreateFlockCommandHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockFlockRepository = _fixture.Freeze<Mock<IFlockRepository>>();
        _mockCoopRepository = _fixture.Freeze<Mock<ICoopRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<CreateFlockCommandHandler>>>();

        _handler = new CreateFlockCommandHandler(
            _mockFlockRepository.Object,
            _mockCoopRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateFlockSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Spring 2024",
            HatchDate = DateTime.UtcNow.AddDays(-30),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 5,
            Notes = "First batch"
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        var createdFlock = Flock.Create(
            tenantId,
            coopId,
            command.Identifier,
            command.HatchDate,
            command.InitialHens,
            command.InitialRoosters,
            command.InitialChicks,
            command.Notes);

        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .ReturnsAsync(createdFlock);

        var expectedDto = new FlockDto
        {
            Id = createdFlock.Id,
            TenantId = tenantId,
            CoopId = coopId,
            Identifier = command.Identifier,
            HatchDate = command.HatchDate,
            CurrentHens = command.InitialHens,
            CurrentRoosters = command.InitialRoosters,
            CurrentChicks = command.InitialChicks,
            IsActive = true
        };

        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Identifier.Should().Be(command.Identifier);
        result.Value.HatchDate.Should().Be(command.HatchDate);
        result.Value.CurrentHens.Should().Be(command.InitialHens);
        result.Value.CurrentRoosters.Should().Be(command.InitialRoosters);
        result.Value.CurrentChicks.Should().Be(command.InitialChicks);
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.IsActive.Should().BeTrue();

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockFlockRepository.Verify(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier), Times.Once);
        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Once);
        _mockMapper.Verify(x => x.Map<FlockDto>(It.IsAny<Flock>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOnlyHens_ShouldCreateFlockSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Hens Only",
            HatchDate = DateTime.UtcNow.AddDays(-60),
            InitialHens = 20,
            InitialRoosters = 0,
            InitialChicks = 0
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        var createdFlock = Flock.Create(
            tenantId,
            coopId,
            command.Identifier,
            command.HatchDate,
            command.InitialHens,
            command.InitialRoosters,
            command.InitialChicks);

        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .ReturnsAsync(createdFlock);

        var expectedDto = new FlockDto
        {
            Id = createdFlock.Id,
            CurrentHens = 20,
            CurrentRoosters = 0,
            CurrentChicks = 0
        };

        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CurrentHens.Should().Be(20);
        result.Value.CurrentRoosters.Should().Be(0);
        result.Value.CurrentChicks.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithoutNotes_ShouldCreateFlockSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "No Notes Batch",
            HatchDate = DateTime.UtcNow.AddDays(-15),
            InitialHens = 5,
            InitialRoosters = 1,
            InitialChicks = 0,
            Notes = null
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        var createdFlock = Flock.Create(
            tenantId,
            coopId,
            command.Identifier,
            command.HatchDate,
            command.InitialHens,
            command.InitialRoosters,
            command.InitialChicks,
            command.Notes);

        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .ReturnsAsync(createdFlock);

        var expectedDto = new FlockDto { Id = createdFlock.Id };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Initial History Creation Tests

    [Fact]
    public async Task Handle_WhenCreatingFlock_ShouldCreateInitialHistoryEntry()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 8,
            InitialRoosters = 1,
            InitialChicks = 3
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        Flock? capturedFlock = null;
        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = Guid.NewGuid() };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedFlock.Should().NotBeNull();
        capturedFlock!.History.Should().HaveCount(1);

        var initialHistory = capturedFlock.History.First();
        initialHistory.Hens.Should().Be(command.InitialHens);
        initialHistory.Roosters.Should().Be(command.InitialRoosters);
        initialHistory.Chicks.Should().Be(command.InitialChicks);
        initialHistory.Reason.Should().Be("Initial");
    }

    [Fact]
    public async Task Handle_WhenCreatingFlock_InitialHistoryShouldHaveTypeInitial()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 5,
            InitialRoosters = 1,
            InitialChicks = 2
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        Flock? capturedFlock = null;
        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = Guid.NewGuid() };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedFlock.Should().NotBeNull();
        capturedFlock!.History.Should().HaveCount(1);

        var initialHistory = capturedFlock.History.First();
        initialHistory.Reason.Should().Be("Initial", "initial history entry should have 'Initial' as the reason");
    }

    [Fact]
    public async Task Handle_WhenCreatingFlock_InitialHistoryShouldHaveCorrectDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var beforeCreate = DateTime.UtcNow;

        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 5,
            InitialRoosters = 1,
            InitialChicks = 2
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        Flock? capturedFlock = null;
        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = Guid.NewGuid() };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedFlock.Should().NotBeNull();

        var initialHistory = capturedFlock!.History.First();
        initialHistory.ChangeDate.Should().BeOnOrAfter(beforeCreate, "history change date should be at or after creation");
        initialHistory.ChangeDate.Should().BeOnOrBefore(afterCreate, "history change date should be at or before completion");
    }

    [Fact]
    public async Task Handle_WhenCreatingFlock_InitialHistoryShouldHaveCorrectComposition()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 12,
            InitialRoosters = 3,
            InitialChicks = 7
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        Flock? capturedFlock = null;
        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = Guid.NewGuid() };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedFlock.Should().NotBeNull();

        var initialHistory = capturedFlock!.History.First();
        initialHistory.Hens.Should().Be(command.InitialHens, "history hens should match initial hens");
        initialHistory.Roosters.Should().Be(command.InitialRoosters, "history roosters should match initial roosters");
        initialHistory.Chicks.Should().Be(command.InitialChicks, "history chicks should match initial chicks");
    }

    [Fact]
    public async Task Handle_WhenCreatingFlock_InitialHistoryShouldBelongToCorrectTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 5,
            InitialRoosters = 1,
            InitialChicks = 2
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        Flock? capturedFlock = null;
        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = Guid.NewGuid() };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedFlock.Should().NotBeNull();

        var initialHistory = capturedFlock!.History.First();
        initialHistory.TenantId.Should().Be(tenantId, "history entry should belong to the same tenant as the flock");
        initialHistory.FlockId.Should().Be(capturedFlock.Id, "history entry should reference the correct flock");
    }

    [Fact]
    public async Task Handle_WhenCreatingFlockWithNotes_InitialHistoryShouldIncludeNotes()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var notes = "First batch - very healthy";

        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 5,
            InitialRoosters = 1,
            InitialChicks = 2,
            Notes = notes
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        Flock? capturedFlock = null;
        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .Callback<Flock>(f => capturedFlock = f)
            .ReturnsAsync((Flock f) => f);

        var expectedDto = new FlockDto { Id = Guid.NewGuid() };
        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedFlock.Should().NotBeNull();

        var initialHistory = capturedFlock!.History.First();
        initialHistory.Notes.Should().Be(notes, "history entry should preserve the initial notes");
    }

    #endregion

    #region Coop Validation Tests

    [Fact]
    public async Task Handle_WhenCoopDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId))
            .ReturnsAsync((Coop?)null); // Coop not found

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Coop not found");

        _mockCoopRepository.Verify(x => x.GetByIdAsync(coopId), Times.Once);
        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    #endregion

    #region Duplicate Identifier Tests

    [Fact]
    public async Task Handle_WithDuplicateIdentifierInSameCoop_ShouldReturnConflictError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Duplicate Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(true); // Simulate duplicate identifier

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Conflict");
        result.Error.Message.Should().Be("A flock with this identifier already exists in the coop");

        _mockFlockRepository.Verify(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier), Times.Once);
        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateIdentifierInDifferentCoop_ShouldCreateSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coop1Id = Guid.NewGuid();
        var coop2Id = Guid.NewGuid();
        var sharedIdentifier = "Shared Flock Name";

        var command = new CreateFlockCommand
        {
            CoopId = coop2Id,
            Identifier = sharedIdentifier, // Same identifier as in coop1
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        var coop2 = Coop.Create(tenantId, "Second Coop", "South Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coop2Id)).ReturnsAsync(coop2);

        // The identifier exists in coop1, but not in coop2
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coop2Id, sharedIdentifier))
            .ReturnsAsync(false);

        var createdFlock = Flock.Create(
            tenantId,
            coop2Id,
            command.Identifier,
            command.HatchDate,
            command.InitialHens,
            command.InitialRoosters,
            command.InitialChicks);

        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .ReturnsAsync(createdFlock);

        var expectedDto = new FlockDto
        {
            Id = createdFlock.Id,
            TenantId = tenantId,
            CoopId = coop2Id,
            Identifier = command.Identifier,
            IsActive = true
        };

        _mockMapper.Setup(x => x.Map<FlockDto>(It.IsAny<Flock>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Identifier.Should().Be(sharedIdentifier);
        result.Value.CoopId.Should().Be(coop2Id);

        _mockFlockRepository.Verify(x => x.ExistsByIdentifierInCoopAsync(coop2Id, sharedIdentifier), Times.Once);
        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Once);
    }

    #endregion

    #region Authentication and Tenant Isolation Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var command = new CreateFlockCommand
        {
            CoopId = Guid.NewGuid(),
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
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
        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task Handle_WithFutureHatchDate_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Future Flock",
            HatchDate = DateTime.UtcNow.AddDays(10), // Future date
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be in the future");

        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeHensCount_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Invalid Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = -5, // Negative count
            InitialRoosters = 2,
            InitialChicks = 0
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be negative");

        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeRoostersCount_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Invalid Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 5,
            InitialRoosters = -2, // Negative count
            InitialChicks = 0
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be negative");

        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeChicksCount_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Invalid Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 5,
            InitialRoosters = 2,
            InitialChicks = -3 // Negative count
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot be negative");

        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAllZeroCounts_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Empty Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 0,
            InitialRoosters = 0,
            InitialChicks = 0
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("At least one animal type must have a count greater than 0");

        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyOrWhitespaceIdentifier_ShouldReturnValidationError(string invalidIdentifier)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = invalidIdentifier,
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("Identifier cannot be empty");

        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIdentifierExceeding50Characters_ShouldReturnValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var longIdentifier = new string('A', 51);
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = longIdentifier,
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
        result.Error.Message.Should().Contain("cannot exceed 50 characters");

        _mockFlockRepository.Verify(x => x.AddAsync(It.IsAny<Flock>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var command = new CreateFlockCommand
        {
            CoopId = coopId,
            Identifier = "Test Flock",
            HatchDate = DateTime.UtcNow.AddDays(-10),
            InitialHens = 10,
            InitialRoosters = 2,
            InitialChicks = 0
        };

        var coop = Coop.Create(tenantId, "Main Coop", "North Field");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockCoopRepository.Setup(x => x.GetByIdAsync(coopId)).ReturnsAsync(coop);
        _mockFlockRepository.Setup(x => x.ExistsByIdentifierInCoopAsync(coopId, command.Identifier))
            .ReturnsAsync(false);
        _mockFlockRepository.Setup(x => x.AddAsync(It.IsAny<Flock>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to create flock");
    }

    #endregion
}
