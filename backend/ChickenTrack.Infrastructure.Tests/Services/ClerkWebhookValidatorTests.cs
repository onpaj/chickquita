using System.Net;
using System.Text.Json;
using ChickenTrack.Application.DTOs;
using ChickenTrack.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Svix;
using Xunit;

namespace ChickenTrack.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for ClerkWebhookValidator.
/// Tests webhook signature validation using Svix.
/// </summary>
public class ClerkWebhookValidatorTests
{
    // Valid Svix webhook secret format (base64 encoded)
    private const string TestWebhookSecret = "whsec_MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw";

    /// <summary>
    /// Creates a mock configuration with the webhook secret.
    /// </summary>
    private static IConfiguration CreateMockConfiguration()
    {
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(c => c["Clerk:WebhookSecret"])
            .Returns(TestWebhookSecret);
        return configurationMock.Object;
    }

    /// <summary>
    /// Creates a valid test webhook payload.
    /// </summary>
    private static string CreateTestPayload()
    {
        var webhookDto = new ClerkWebhookDto
        {
            Type = "user.created",
            Data = new ClerkWebhookDataDto
            {
                Id = "user_test123",
                EmailAddresses = new List<ClerkEmailDto>
                {
                    new()
                    {
                        Id = "email_test123",
                        EmailAddress = "test@example.com",
                        Verified = true
                    }
                },
                PrimaryEmailAddressId = "email_test123",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        };

        return JsonSerializer.Serialize(webhookDto);
    }

    /// <summary>
    /// Generates valid Svix webhook headers for a given payload.
    /// This uses the actual Svix library to create authentic signatures for testing.
    /// </summary>
    private static Dictionary<string, string> GenerateValidHeaders(string payload, string secret)
    {
        var webhook = new Webhook(secret);
        var messageId = $"msg_{Guid.NewGuid():N}";
        var timestamp = DateTimeOffset.UtcNow;

        // Generate a valid signature using Svix's Sign method
        var signature = webhook.Sign(messageId, timestamp, payload);

        return new Dictionary<string, string>
        {
            { "svix-id", messageId },
            { "svix-timestamp", timestamp.ToUnixTimeSeconds().ToString() },
            { "svix-signature", signature }
        };
    }

    [Fact]
    public void Constructor_WhenWebhookSecretIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(c => c["Clerk:WebhookSecret"])
            .Returns((string?)null);

        // Act
        var act = () => new ClerkWebhookValidator(configurationMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Clerk webhook secret is not configured");
    }

    [Fact]
    public void ValidateWebhook_WithValidSignature_ReturnsSuccess()
    {
        // Arrange
        var configuration = CreateMockConfiguration();
        var validator = new ClerkWebhookValidator(configuration);
        var payload = CreateTestPayload();
        var headers = GenerateValidHeaders(payload, TestWebhookSecret);

        // Act
        var result = validator.ValidateWebhook(payload, headers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Type.Should().Be("user.created");
        result.Value.Data.Id.Should().Be("user_test123");
        result.Value.Data.EmailAddresses.Should().HaveCount(1);
        result.Value.Data.EmailAddresses[0].EmailAddress.Should().Be("test@example.com");
    }

    [Fact]
    public void ValidateWebhook_WithTamperedPayload_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateMockConfiguration();
        var validator = new ClerkWebhookValidator(configuration);
        var originalPayload = CreateTestPayload();
        var headers = GenerateValidHeaders(originalPayload, TestWebhookSecret);

        // Tamper with the payload after signature was generated
        var tamperedPayload = originalPayload.Replace("user_test123", "user_hacker999");

        // Act
        var result = validator.ValidateWebhook(tamperedPayload, headers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("INVALID_SIGNATURE");
        result.Error.Message.Should().Contain("Webhook signature validation failed");
    }

    [Fact]
    public void ValidateWebhook_WithInvalidSignature_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateMockConfiguration();
        var validator = new ClerkWebhookValidator(configuration);
        var payload = CreateTestPayload();

        // Create headers with invalid signature
        var headers = new Dictionary<string, string>
        {
            { "svix-id", $"msg_{Guid.NewGuid():N}" },
            { "svix-timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            { "svix-signature", "v1,invalid_signature_here" }
        };

        // Act
        var result = validator.ValidateWebhook(payload, headers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("INVALID_SIGNATURE");
    }

    [Fact]
    public void ValidateWebhook_WithMissingSvixIdHeader_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateMockConfiguration();
        var validator = new ClerkWebhookValidator(configuration);
        var payload = CreateTestPayload();

        var headers = new Dictionary<string, string>
        {
            { "svix-timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            { "svix-signature", "v1,BpniOSYVJrA..." }
            // Missing svix-id
        };

        // Act
        var result = validator.ValidateWebhook(payload, headers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("MISSING_HEADERS");
        result.Error.Message.Should().Be("Required Svix headers are missing");
    }

    [Fact]
    public void ValidateWebhook_WithMissingSvixTimestampHeader_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateMockConfiguration();
        var validator = new ClerkWebhookValidator(configuration);
        var payload = CreateTestPayload();

        var headers = new Dictionary<string, string>
        {
            { "svix-id", $"msg_{Guid.NewGuid():N}" },
            { "svix-signature", "v1,BpniOSYVJrA..." }
            // Missing svix-timestamp
        };

        // Act
        var result = validator.ValidateWebhook(payload, headers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("MISSING_HEADERS");
    }

    [Fact]
    public void ValidateWebhook_WithMissingSvixSignatureHeader_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateMockConfiguration();
        var validator = new ClerkWebhookValidator(configuration);
        var payload = CreateTestPayload();

        var headers = new Dictionary<string, string>
        {
            { "svix-id", $"msg_{Guid.NewGuid():N}" },
            { "svix-timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
            // Missing svix-signature
        };

        // Act
        var result = validator.ValidateWebhook(payload, headers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("MISSING_HEADERS");
    }

    [Fact]
    public void ValidateWebhook_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var configuration = CreateMockConfiguration();
        var validator = new ClerkWebhookValidator(configuration);
        var invalidPayload = "{ invalid json }";
        var headers = GenerateValidHeaders(invalidPayload, TestWebhookSecret);

        // Act
        var result = validator.ValidateWebhook(invalidPayload, headers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("INVALID_JSON");
        result.Error.Message.Should().Contain("Invalid JSON payload");
    }

    [Fact]
    public void ValidateWebhook_WithDifferentEventTypes_ExtractsCorrectly()
    {
        // Arrange
        var configuration = CreateMockConfiguration();
        var validator = new ClerkWebhookValidator(configuration);

        var webhookDto = new ClerkWebhookDto
        {
            Type = "user.updated",
            Data = new ClerkWebhookDataDto
            {
                Id = "user_updated123",
                EmailAddresses = new List<ClerkEmailDto>
                {
                    new()
                    {
                        Id = "email_new123",
                        EmailAddress = "updated@example.com",
                        Verified = true
                    }
                },
                PrimaryEmailAddressId = "email_new123",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        };

        var payload = JsonSerializer.Serialize(webhookDto);
        var headers = GenerateValidHeaders(payload, TestWebhookSecret);

        // Act
        var result = validator.ValidateWebhook(payload, headers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("user.updated");
        result.Value.Data.Id.Should().Be("user_updated123");
        result.Value.Data.EmailAddresses[0].EmailAddress.Should().Be("updated@example.com");
    }
}
