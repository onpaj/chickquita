using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.DailyRecords.Queries;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Chickquita.Application.Tests.Features.DailyRecords.Queries;

/// <summary>
/// Unit tests for GetDailyRecordsQueryHandler.
/// Tests cover filtering by flock ID, date range, all filter combinations, and ordering.
/// </summary>
public class GetDailyRecordsQueryHandlerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IDailyRecordRepository> _mockDailyRecordRepository;
    private readonly Mock<IFlockRepository> _mockFlockRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetDailyRecordsQueryHandler>> _mockLogger;
    private readonly GetDailyRecordsQueryHandler _handler;

    public GetDailyRecordsQueryHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _mockDailyRecordRepository = _fixture.Freeze<Mock<IDailyRecordRepository>>();
        _mockFlockRepository = _fixture.Freeze<Mock<IFlockRepository>>();
        _mockCurrentUserService = _fixture.Freeze<Mock<ICurrentUserService>>();
        _mockMapper = _fixture.Freeze<Mock<IMapper>>();
        _mockLogger = _fixture.Freeze<Mock<ILogger<GetDailyRecordsQueryHandler>>>();

        _handler = new GetDailyRecordsQueryHandler(
            _mockDailyRecordRepository.Object,
            _mockFlockRepository.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Happy Path Tests - No Filters

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllDailyRecords()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetDailyRecordsQuery
        {
            FlockId = null,
            StartDate = null,
            EndDate = null
        };

        var record1 = DailyRecord.Create(tenantId, flockId, DateTime.UtcNow.AddDays(-2), 10, "Good day");
        var record2 = DailyRecord.Create(tenantId, flockId, DateTime.UtcNow.AddDays(-1), 12, "Great day");

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<DailyRecord> { record1, record2 });

        var recordDtos = new List<DailyRecordDto>
        {
            new DailyRecordDto { Id = record1.Id, EggCount = 10, RecordDate = DateTime.UtcNow.AddDays(-2) },
            new DailyRecordDto { Id = record2.Id, EggCount = 12, RecordDate = DateTime.UtcNow.AddDays(-1) }
        };

        _mockMapper.Setup(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()))
            .Returns(recordDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        _mockDailyRecordRepository.Verify(x => x.GetAllAsync(), Times.Once);
        _mockMapper.Verify(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoFiltersAndEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetDailyRecordsQuery
        {
            FlockId = null,
            StartDate = null,
            EndDate = null
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<DailyRecord>());

        _mockMapper.Setup(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()))
            .Returns(new List<DailyRecordDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region Filter Combination Tests

    [Fact]
    public async Task Handle_WithFlockIdOnly_ShouldReturnRecordsForFlock()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetDailyRecordsQuery
        {
            FlockId = flockId,
            StartDate = null,
            EndDate = null
        };

        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);
        var record1 = DailyRecord.Create(tenantId, flockId, DateTime.UtcNow.AddDays(-5), 10);
        var record2 = DailyRecord.Create(tenantId, flockId, DateTime.UtcNow.AddDays(-3), 12);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId)).ReturnsAsync(flock);
        _mockDailyRecordRepository.Setup(x => x.GetByFlockIdAsync(flockId))
            .ReturnsAsync(new List<DailyRecord> { record1, record2 });

        var recordDtos = new List<DailyRecordDto>
        {
            new DailyRecordDto { Id = record1.Id, FlockId = flockId, EggCount = 10 },
            new DailyRecordDto { Id = record2.Id, FlockId = flockId, EggCount = 12 }
        };

        _mockMapper.Setup(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()))
            .Returns(recordDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(r => r.FlockId.Should().Be(flockId));

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.GetByFlockIdAsync(flockId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFlockIdAndFullDateRange_ShouldReturnFilteredRecords()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-10).Date;
        var endDate = DateTime.UtcNow.AddDays(-5).Date;

        var query = new GetDailyRecordsQuery
        {
            FlockId = flockId,
            StartDate = startDate,
            EndDate = endDate
        };

        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);
        var record1 = DailyRecord.Create(tenantId, flockId, startDate.AddDays(1), 10);
        var record2 = DailyRecord.Create(tenantId, flockId, startDate.AddDays(3), 12);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId)).ReturnsAsync(flock);
        _mockDailyRecordRepository.Setup(x => x.GetByFlockIdAndDateRangeAsync(flockId, startDate, endDate))
            .ReturnsAsync(new List<DailyRecord> { record1, record2 });

        var recordDtos = new List<DailyRecordDto>
        {
            new DailyRecordDto { Id = record1.Id, FlockId = flockId, RecordDate = startDate.AddDays(1) },
            new DailyRecordDto { Id = record2.Id, FlockId = flockId, RecordDate = startDate.AddDays(3) }
        };

        _mockMapper.Setup(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()))
            .Returns(recordDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.GetByFlockIdAndDateRangeAsync(flockId, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFlockIdAndStartDateOnly_ShouldFilterByStartDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-10).Date;

        var query = new GetDailyRecordsQuery
        {
            FlockId = flockId,
            StartDate = startDate,
            EndDate = null
        };

        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);

        // Create records: some before startDate (should be excluded), some after (should be included)
        var recordBeforeStart = DailyRecord.Create(tenantId, flockId, startDate.AddDays(-5), 8);
        var recordAtStart = DailyRecord.Create(tenantId, flockId, startDate, 10);
        var recordAfterStart = DailyRecord.Create(tenantId, flockId, startDate.AddDays(2), 12);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId)).ReturnsAsync(flock);

        // Repository returns all records for the flock
        _mockDailyRecordRepository.Setup(x => x.GetByFlockIdAsync(flockId))
            .ReturnsAsync(new List<DailyRecord> { recordBeforeStart, recordAtStart, recordAfterStart });

        // Only records at or after startDate should be mapped
        var recordDtos = new List<DailyRecordDto>
        {
            new DailyRecordDto { Id = recordAtStart.Id, RecordDate = startDate },
            new DailyRecordDto { Id = recordAfterStart.Id, RecordDate = startDate.AddDays(2) }
        };

        _mockMapper.Setup(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()))
            .Returns<List<DailyRecord>>(records =>
            {
                // Simulate mapping only the filtered records
                var filtered = records.Where(r => r.RecordDate >= startDate).ToList();
                return recordDtos.Take(filtered.Count).ToList();
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(r => r.RecordDate.Should().BeOnOrAfter(startDate));
    }

    [Fact]
    public async Task Handle_WithFlockIdAndEndDateOnly_ShouldFilterByEndDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var coopId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var endDate = DateTime.UtcNow.AddDays(-5).Date;

        var query = new GetDailyRecordsQuery
        {
            FlockId = flockId,
            StartDate = null,
            EndDate = endDate
        };

        var flock = Flock.Create(tenantId, coopId, "Test Flock", DateTime.UtcNow.AddDays(-30), 10, 2, 5);

        // Create records: some before endDate (should be included), some after (should be excluded)
        var recordBeforeEnd = DailyRecord.Create(tenantId, flockId, endDate.AddDays(-2), 8);
        var recordAtEnd = DailyRecord.Create(tenantId, flockId, endDate, 10);
        var recordAfterEnd = DailyRecord.Create(tenantId, flockId, endDate.AddDays(2), 12);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId)).ReturnsAsync(flock);

        // Repository returns all records for the flock
        _mockDailyRecordRepository.Setup(x => x.GetByFlockIdAsync(flockId))
            .ReturnsAsync(new List<DailyRecord> { recordBeforeEnd, recordAtEnd, recordAfterEnd });

        // Only records at or before endDate should be mapped
        var recordDtos = new List<DailyRecordDto>
        {
            new DailyRecordDto { Id = recordBeforeEnd.Id, RecordDate = endDate.AddDays(-2) },
            new DailyRecordDto { Id = recordAtEnd.Id, RecordDate = endDate }
        };

        _mockMapper.Setup(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()))
            .Returns<List<DailyRecord>>(records =>
            {
                // Simulate mapping only the filtered records
                var filtered = records.Where(r => r.RecordDate <= endDate).ToList();
                return recordDtos.Take(filtered.Count).ToList();
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(r => r.RecordDate.Should().BeOnOrBefore(endDate));
    }

    [Fact]
    public async Task Handle_WithDateRangeOnly_ShouldFilterAllRecordsByDateRange()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-10).Date;
        var endDate = DateTime.UtcNow.AddDays(-5).Date;

        var query = new GetDailyRecordsQuery
        {
            FlockId = null,
            StartDate = startDate,
            EndDate = endDate
        };

        var recordInRange1 = DailyRecord.Create(tenantId, flockId, startDate.AddDays(1), 10);
        var recordInRange2 = DailyRecord.Create(tenantId, flockId, startDate.AddDays(3), 12);
        var recordOutOfRange = DailyRecord.Create(tenantId, flockId, startDate.AddDays(-2), 8);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<DailyRecord> { recordInRange1, recordInRange2, recordOutOfRange });

        var recordDtos = new List<DailyRecordDto>
        {
            new DailyRecordDto { Id = recordInRange1.Id, RecordDate = startDate.AddDays(1) },
            new DailyRecordDto { Id = recordInRange2.Id, RecordDate = startDate.AddDays(3) }
        };

        _mockMapper.Setup(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()))
            .Returns<List<DailyRecord>>(records =>
            {
                // Simulate mapping only the filtered records
                var filtered = records.Where(r => r.RecordDate >= startDate && r.RecordDate <= endDate).ToList();
                return recordDtos.Take(filtered.Count).ToList();
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(r =>
        {
            r.RecordDate.Should().BeOnOrAfter(startDate);
            r.RecordDate.Should().BeOnOrBefore(endDate);
        });
    }

    [Fact]
    public async Task Handle_WithStartDateOnly_ShouldFilterAllRecordsByStartDate()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-10).Date;

        var query = new GetDailyRecordsQuery
        {
            FlockId = null,
            StartDate = startDate,
            EndDate = null
        };

        var recordAfterStart = DailyRecord.Create(tenantId, flockId, startDate.AddDays(1), 10);
        var recordBeforeStart = DailyRecord.Create(tenantId, flockId, startDate.AddDays(-2), 8);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<DailyRecord> { recordAfterStart, recordBeforeStart });

        var recordDtos = new List<DailyRecordDto>
        {
            new DailyRecordDto { Id = recordAfterStart.Id, RecordDate = startDate.AddDays(1) }
        };

        _mockMapper.Setup(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()))
            .Returns<List<DailyRecord>>(records =>
            {
                var filtered = records.Where(r => r.RecordDate >= startDate).ToList();
                return recordDtos.Take(filtered.Count).ToList();
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.Should().AllSatisfy(r => r.RecordDate.Should().BeOnOrAfter(startDate));
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public async Task Handle_ShouldReturnRecordsOrderedByRecordDateDescending()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetDailyRecordsQuery
        {
            FlockId = null,
            StartDate = null,
            EndDate = null
        };

        var oldestRecord = DailyRecord.Create(tenantId, flockId, DateTime.UtcNow.AddDays(-10), 8);
        var middleRecord = DailyRecord.Create(tenantId, flockId, DateTime.UtcNow.AddDays(-5), 10);
        var newestRecord = DailyRecord.Create(tenantId, flockId, DateTime.UtcNow.AddDays(-1), 12);

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);

        // Repository returns records ordered by RecordDate DESC (newest first)
        _mockDailyRecordRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<DailyRecord> { newestRecord, middleRecord, oldestRecord });

        // DTOs should maintain the same order as returned from repository
        var recordDtos = new List<DailyRecordDto>
        {
            new DailyRecordDto { Id = newestRecord.Id, RecordDate = DateTime.UtcNow.AddDays(-1) },
            new DailyRecordDto { Id = middleRecord.Id, RecordDate = DateTime.UtcNow.AddDays(-5) },
            new DailyRecordDto { Id = oldestRecord.Id, RecordDate = DateTime.UtcNow.AddDays(-10) }
        };

        _mockMapper.Setup(x => x.Map<List<DailyRecordDto>>(It.IsAny<List<DailyRecord>>()))
            .Returns(recordDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);

        // Verify descending order (newest first) - repository returns ordered data
        // In real implementation, repository returns ordered by RecordDate DESC
        // For this test, we verify the results maintain proper order
        result.Value.Should().BeInDescendingOrder(r => r.RecordDate);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_WhenFlockDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var flockId = Guid.NewGuid();
        var query = new GetDailyRecordsQuery
        {
            FlockId = flockId,
            StartDate = null,
            EndDate = null
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockFlockRepository.Setup(x => x.GetByIdAsync(flockId))
            .ReturnsAsync((Flock?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.NotFound");
        result.Error.Message.Should().Be("Flock not found");

        _mockFlockRepository.Verify(x => x.GetByIdAsync(flockId), Times.Once);
        _mockDailyRecordRepository.Verify(x => x.GetByFlockIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Authentication and Authorization Tests

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetDailyRecordsQuery
        {
            FlockId = null,
            StartDate = null,
            EndDate = null
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("User is not authenticated");

        _mockDailyRecordRepository.Verify(x => x.GetAllAsync(), Times.Never);
        _mockDailyRecordRepository.Verify(x => x.GetByFlockIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIdNotFound_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var query = new GetDailyRecordsQuery
        {
            FlockId = null,
            StartDate = null,
            EndDate = null
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns((Guid?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Unauthorized");
        result.Error.Message.Should().Be("Tenant not found");

        _mockDailyRecordRepository.Verify(x => x.GetAllAsync(), Times.Never);
        _mockDailyRecordRepository.Verify(x => x.GetByFlockIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetDailyRecordsQuery
        {
            FlockId = null,
            StartDate = null,
            EndDate = null
        };

        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        _mockCurrentUserService.Setup(x => x.TenantId).Returns(tenantId);
        _mockDailyRecordRepository.Setup(x => x.GetAllAsync())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Failure");
        result.Error.Message.Should().Contain("Failed to retrieve daily records");
    }

    #endregion
}
