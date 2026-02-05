using System.Net;
using System.Text;
using ChickenTrack.Application.DTOs;
using ChickenTrack.Application.Features.Users.Commands;
using ChickenTrack.Application.Interfaces;
using ChickenTrack.Domain.Common;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ChickenTrack.Api.Tests.Endpoints;

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

        var requestBody = "{\"type\":\"user.created\",\"data\":{\"id\":\"user_123\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/webhooks/clerk", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ClerkWebhook_WithValidSignatureAndUserCreatedEvent_DispatchesSyncUserCommand()
    {
        // Arrange
        var mockValidator = new Mock<IClerkWebhookValidator>();
        var mockMediator = new Mock<IMediator>();

        var webhookDto = new ClerkWebhookDto
        {
            Type = "user.created",
            Data = new ClerkWebhookDataDto
            {
                Id = "user_123",
                EmailAddresses = new List<ClerkEmailDto>
                {
                    new ClerkEmailDto
                    {
                        Id = "email_1",
                        EmailAddress = "test@example.com",
                        Verified = true
                    }
                },
                PrimaryEmailAddressId = "email_1"
            }
        };

        mockValidator
            .Setup(v => v.ValidateWebhook(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
            .Returns(Result<ClerkWebhookDto>.Success(webhookDto));

        mockMediator
            .Setup(m => m.Send(It.IsAny<SyncUserCommand>(), It.IsAny<CancellationToken>()))
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

        var requestBody = "{\"type\":\"user.created\",\"data\":{\"id\":\"user_123\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/webhooks/clerk", content);

        // Assert
        mockMediator.Verify(
            m => m.Send(
                It.Is<SyncUserCommand>(cmd =>
                    cmd.ClerkUserId == "user_123" &&
                    cmd.Email == "test@example.com"),
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
            Type = "user.created",
            Data = new ClerkWebhookDataDto
            {
                Id = "user_123",
                EmailAddresses = new List<ClerkEmailDto>
                {
                    new ClerkEmailDto
                    {
                        Id = "email_1",
                        EmailAddress = "test@example.com",
                        Verified = true
                    }
                },
                PrimaryEmailAddressId = "email_1"
            }
        };

        mockValidator
            .Setup(v => v.ValidateWebhook(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
            .Returns(Result<ClerkWebhookDto>.Success(webhookDto));

        mockMediator
            .Setup(m => m.Send(It.IsAny<SyncUserCommand>(), It.IsAny<CancellationToken>()))
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

        var requestBody = "{\"type\":\"user.created\",\"data\":{\"id\":\"user_123\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/webhooks/clerk", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
