using System.Net;
using System.Text;
using Chickquita.Application.DTOs;
using Chickquita.Application.Features.Users.Commands;
using Chickquita.Application.Interfaces;
using Chickquita.Domain.Common;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Chickquita.Api.Tests.Endpoints;

/// <summary>
/// Tests for Clerk webhook endpoint.
/// </summary>
public class ClerkWebhookEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ClerkWebhookEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ClerkWebhook_WithInvalidSignature_Returns401Unauthorized()
    {
        // Arrange
        var mockValidator = new Mock<IClerkWebhookValidator>();
        mockValidator
            .Setup(v => v.ValidateWebhook(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
            .Returns(Result<ClerkWebhookDto>.Failure(new Error("Webhook.InvalidSignature", "Invalid signature")));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing IClerkWebhookValidator registration if any
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClerkWebhookValidator));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add mock validator
                services.AddScoped(_ => mockValidator.Object);
            });
        }).CreateClient();

        var requestBody = "{\"type\":\"organization.created\",\"data\":{\"id\":\"org_abc123\",\"name\":\"Smith Farm\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/webhooks/clerk", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ClerkWebhook_WithValidSignatureAndOrgCreatedEvent_DispatchesSyncOrgCommand()
    {
        // Arrange
        var mockValidator = new Mock<IClerkWebhookValidator>();
        var mockMediator = new Mock<IMediator>();

        var webhookDto = new ClerkWebhookDto
        {
            Type = "organization.created",
            Data = new ClerkWebhookDataDto
            {
                Id = "org_abc123",
                Name = "Smith Farm"
            }
        };

        mockValidator
            .Setup(v => v.ValidateWebhook(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
            .Returns(Result<ClerkWebhookDto>.Success(webhookDto));

        mockMediator
            .Setup(m => m.Send(It.IsAny<SyncOrgCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TenantDto>.Success(new TenantDto()));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing registrations
                var validatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClerkWebhookValidator));
                if (validatorDescriptor != null)
                {
                    services.Remove(validatorDescriptor);
                }

                var mediatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (mediatorDescriptor != null)
                {
                    services.Remove(mediatorDescriptor);
                }

                // Add mocks
                services.AddScoped(_ => mockValidator.Object);
                services.AddScoped(_ => mockMediator.Object);
            });
        }).CreateClient();

        var requestBody = "{\"type\":\"organization.created\",\"data\":{\"id\":\"org_abc123\",\"name\":\"Smith Farm\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        await client.PostAsync("/api/webhooks/clerk", content);

        // Assert: SyncOrgCommand dispatched with correct data
        // NOTE: This verification will pass once Task 4 wires up the organization.created handler.
        // Currently the endpoint stub does not yet dispatch SyncOrgCommand.
        mockMediator.Verify(
            m => m.Send(
                It.Is<SyncOrgCommand>(cmd =>
                    cmd.ClerkOrgId == "org_abc123" &&
                    cmd.Name == "Smith Farm"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ClerkWebhook_WithValidSignatureAndSuccessfulSync_Returns200OK()
    {
        // Arrange
        var mockValidator = new Mock<IClerkWebhookValidator>();
        var mockMediator = new Mock<IMediator>();

        var webhookDto = new ClerkWebhookDto
        {
            Type = "organization.created",
            Data = new ClerkWebhookDataDto
            {
                Id = "org_abc123",
                Name = "Smith Farm"
            }
        };

        mockValidator
            .Setup(v => v.ValidateWebhook(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
            .Returns(Result<ClerkWebhookDto>.Success(webhookDto));

        mockMediator
            .Setup(m => m.Send(It.IsAny<SyncOrgCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TenantDto>.Success(new TenantDto()));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing registrations
                var validatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClerkWebhookValidator));
                if (validatorDescriptor != null)
                {
                    services.Remove(validatorDescriptor);
                }

                var mediatorDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (mediatorDescriptor != null)
                {
                    services.Remove(mediatorDescriptor);
                }

                // Add mocks
                services.AddScoped(_ => mockValidator.Object);
                services.AddScoped(_ => mockMediator.Object);
            });
        }).CreateClient();

        var requestBody = "{\"type\":\"organization.created\",\"data\":{\"id\":\"org_abc123\",\"name\":\"Smith Farm\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/webhooks/clerk", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
