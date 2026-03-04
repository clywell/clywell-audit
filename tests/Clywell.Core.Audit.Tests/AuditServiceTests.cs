namespace Clywell.Core.Audit.Tests;

public class AuditServiceTests
{
    private readonly Mock<IAuditLogger> _loggerMock = new();
    private readonly Mock<IAuditUserProvider> _userProviderMock = new();
    private readonly Mock<IAuditTimestampProvider> _timestampProviderMock = new();

    private AuditService CreateService(
        IAuditUserProvider? userProvider = null,
        IAuditTimestampProvider? timestampProvider = null)
    {
        return new AuditService(
            _loggerMock.Object,
            userProvider ?? _userProviderMock.Object,
            timestampProvider ?? _timestampProviderMock.Object);
    }

    [Fact]
    public async Task LogAsync_Single_ShouldCallLogger()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);
        _timestampProviderMock.Setup(p => p.GetCurrentTimestamp()).Returns(timestamp);
        _userProviderMock.Setup(p => p.GetCurrentUserId()).Returns("user-42");

        var service = CreateService();
        var entry = new AuditEntry
        {
            Action = AuditAction.Created,
            EntityType = "Order",
            EntityId = "1"
        };

        // Act
        await service.LogAsync(entry);

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(e =>
                    e.Count == 1 &&
                    e[0].Action == AuditAction.Created &&
                    e[0].EntityType == "Order" &&
                    e[0].Timestamp == timestamp &&
                    e[0].UserId == "user-42"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_Single_ShouldNotOverwriteExistingUserId()
    {
        // Arrange
        _timestampProviderMock.Setup(p => p.GetCurrentTimestamp()).Returns(DateTimeOffset.UtcNow);
        _userProviderMock.Setup(p => p.GetCurrentUserId()).Returns("provider-user");

        var service = CreateService();
        var entry = new AuditEntry
        {
            Action = AuditAction.Updated,
            EntityType = "Product",
            EntityId = "99",
            UserId = "explicit-user"
        };

        // Act
        await service.LogAsync(entry);

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(e =>
                    e.Count == 1 && e[0].UserId == "explicit-user"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_Single_ShouldNotOverwriteExistingTimestamp()
    {
        // Arrange
        var existingTimestamp = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        _timestampProviderMock.Setup(p => p.GetCurrentTimestamp()).Returns(DateTimeOffset.UtcNow);

        var service = CreateService();
        var entry = new AuditEntry
        {
            Action = AuditAction.Deleted,
            EntityType = "Item",
            EntityId = "5",
            Timestamp = existingTimestamp
        };

        // Act
        await service.LogAsync(entry);

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(e =>
                    e.Count == 1 && e[0].Timestamp == existingTimestamp),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_Batch_ShouldEnrichAllEntries()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 3, 20, 8, 0, 0, TimeSpan.Zero);
        _timestampProviderMock.Setup(p => p.GetCurrentTimestamp()).Returns(timestamp);
        _userProviderMock.Setup(p => p.GetCurrentUserId()).Returns("batch-user");

        var service = CreateService();
        var entries = new List<AuditEntry>
        {
            new() { Action = AuditAction.Created, EntityType = "A", EntityId = "1" },
            new() { Action = AuditAction.Updated, EntityType = "B", EntityId = "2" },
        };

        // Act
        await service.LogAsync(entries);

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(e =>
                    e.Count == 2 &&
                    e.All(x => x.Timestamp == timestamp && x.UserId == "batch-user")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_WithoutUserProvider_ShouldLeaveUserIdNull()
    {
        // Arrange
        _timestampProviderMock.Setup(p => p.GetCurrentTimestamp()).Returns(DateTimeOffset.UtcNow);

        var service = new AuditService(
            _loggerMock.Object,
            userProvider: null,
            timestampProvider: _timestampProviderMock.Object);

        var entry = new AuditEntry
        {
            Action = AuditAction.Read,
            EntityType = "Report",
            EntityId = "7"
        };

        // Act
        await service.LogAsync(entry);

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(e =>
                    e.Count == 1 && e[0].UserId == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_NullEntry_ShouldThrow()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.LogAsync((AuditEntry)null!));
    }

    [Fact]
    public async Task LogAsync_NullEntries_ShouldThrow()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.LogAsync((IReadOnlyList<AuditEntry>)null!));
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new AuditService(null!));
    }
}
