using AutoMapper;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Users.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chickquita.Application.Tests.Features.Users.Commands;

/// <summary>
/// Unit tests for SyncOrgCommandHandler.
/// </summary>
public class SyncOrgCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<SyncOrgCommandHandler>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private SyncOrgCommandHandler CreateHandler()
        => new(_repoMock.Object, _mapperMock.Object, _loggerMock.Object, _unitOfWorkMock.Object);

    [Fact]
    public async Task Handle_NewOrg_CreatesAndReturnsTenant()
    {
        var command = new SyncOrgCommand { ClerkOrgId = "org_abc", Name = "Smith Farm" };
        var created = Tenant.Create("org_abc", "Smith Farm");

        _repoMock.Setup(r => r.GetByClerkOrgIdAsync("org_abc")).ReturnsAsync((Tenant?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Tenant>())).ReturnsAsync(created);
        _mapperMock.Setup(m => m.Map<TenantDto>(created)).Returns(new TenantDto { ClerkOrgId = "org_abc" });

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repoMock.Verify(r => r.AddAsync(It.Is<Tenant>(t => t.ClerkOrgId == "org_abc")), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingOrg_UpdatesNameAndReturnsExisting()
    {
        var command = new SyncOrgCommand { ClerkOrgId = "org_abc", Name = "New Name" };
        var existing = Tenant.Create("org_abc", "Old Name");

        _repoMock.Setup(r => r.GetByClerkOrgIdAsync("org_abc")).ReturnsAsync(existing);
        _mapperMock.Setup(m => m.Map<TenantDto>(existing)).Returns(new TenantDto { ClerkOrgId = "org_abc" });

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existing.Name.Should().Be("New Name");
        _repoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingOrg_SameName_DoesNotCallUpdate()
    {
        var command = new SyncOrgCommand { ClerkOrgId = "org_abc", Name = "Same Name" };
        var existing = Tenant.Create("org_abc", "Same Name");

        _repoMock.Setup(r => r.GetByClerkOrgIdAsync("org_abc")).ReturnsAsync(existing);
        _mapperMock.Setup(m => m.Map<TenantDto>(existing)).Returns(new TenantDto());

        var handler = CreateHandler();
        await handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tenant>()), Times.Never);
    }
}
