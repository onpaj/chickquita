using Chickquita.Application.Interfaces;
using Chickquita.Infrastructure.Data.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Data.Common;

namespace Chickquita.Infrastructure.Tests.Data;

/// <summary>
/// Tests for TenantInterceptor focusing on the sync/async path correctness.
/// The critical invariant: ConnectionOpened (sync) must NOT block on async code.
/// </summary>
public class TenantInterceptorTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<TenantInterceptor>> _loggerMock;
    private readonly TenantInterceptor _interceptor;
    private readonly Guid _tenantId = Guid.NewGuid();

    // The base DbConnectionInterceptor.ConnectionOpened / ConnectionOpenedAsync are both no-ops,
    // so we can safely pass null for the event data in unit tests.
    private static readonly ConnectionEndEventData NullEventData = null!;

    public TenantInterceptorTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<TenantInterceptor>>();
        _interceptor = new TenantInterceptor(_currentUserServiceMock.Object, _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // Sync path
    // -------------------------------------------------------------------------

    [Fact]
    public void ConnectionOpened_WhenTenantExists_CallsExecuteNonQuery()
    {
        _currentUserServiceMock.Setup(s => s.TenantId).Returns(_tenantId);

        var (connectionMock, commandMock, _) = BuildMockedConnection();

        _interceptor.ConnectionOpened(connectionMock.Object, NullEventData);

        commandMock.Verify(c => c.ExecuteNonQuery(), Times.Once);
    }

    [Fact]
    public void ConnectionOpened_WhenTenantExists_SetsCorrectCommandText()
    {
        _currentUserServiceMock.Setup(s => s.TenantId).Returns(_tenantId);

        var (connectionMock, commandMock, _) = BuildMockedConnection();

        _interceptor.ConnectionOpened(connectionMock.Object, NullEventData);

        commandMock.Object.CommandText.Should().Be("SELECT set_tenant_context(@tenantId)");
    }

    [Fact]
    public void ConnectionOpened_WhenNoTenant_DoesNotCreateCommand()
    {
        _currentUserServiceMock.Setup(s => s.TenantId).Returns((Guid?)null);

        var connectionMock = new Mock<DbConnection>();

        _interceptor.ConnectionOpened(connectionMock.Object, NullEventData);

        connectionMock.Protected().Verify("CreateDbCommand", Times.Never());
    }

    // -------------------------------------------------------------------------
    // Async path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ConnectionOpenedAsync_WhenTenantExists_CallsExecuteNonQueryAsync()
    {
        _currentUserServiceMock.Setup(s => s.TenantId).Returns(_tenantId);

        var (connectionMock, commandMock, _) = BuildMockedConnection();

        await _interceptor.ConnectionOpenedAsync(connectionMock.Object, NullEventData);

        commandMock.Verify(
            c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConnectionOpenedAsync_WhenNoTenant_DoesNotCreateCommand()
    {
        _currentUserServiceMock.Setup(s => s.TenantId).Returns((Guid?)null);

        var connectionMock = new Mock<DbConnection>();

        await _interceptor.ConnectionOpenedAsync(connectionMock.Object, NullEventData);

        connectionMock.Protected().Verify("CreateDbCommand", Times.Never());
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static (Mock<DbConnection> connection, Mock<DbCommand> command, Mock<DbParameter> parameter) BuildMockedConnection()
    {
        var paramMock = new Mock<DbParameter>();
        paramMock.SetupAllProperties();

        var paramCollectionMock = new Mock<DbParameterCollection>();
        paramCollectionMock.Setup(p => p.Add(It.IsAny<object>())).Returns(0);

        var commandMock = new Mock<DbCommand>();
        commandMock.SetupAllProperties();
        commandMock.Setup(c => c.ExecuteNonQuery()).Returns(1);
        commandMock
            .Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        commandMock.Protected().Setup<DbParameter>("CreateDbParameter").Returns(paramMock.Object);
        commandMock.Protected()
            .SetupGet<DbParameterCollection>("DbParameterCollection")
            .Returns(paramCollectionMock.Object);

        var connectionMock = new Mock<DbConnection>();
        connectionMock.Protected()
            .Setup<DbCommand>("CreateDbCommand")
            .Returns(commandMock.Object);

        return (connectionMock, commandMock, paramMock);
    }
}
