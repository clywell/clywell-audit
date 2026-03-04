namespace Clywell.Core.Audit.EntityFramework.Tests;

public class AuditSaveChangesInterceptorTests : IDisposable
{
    private readonly Mock<IAuditLogger> _loggerMock = new();
    private readonly Mock<IAuditUserProvider> _userProviderMock = new();
    private readonly Mock<IAuditTimestampProvider> _timestampProviderMock = new();
    private readonly TestDbContext _dbContext;

    private readonly DateTimeOffset _fixedTimestamp = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    public AuditSaveChangesInterceptorTests()
    {
        _timestampProviderMock.Setup(p => p.GetCurrentTimestamp()).Returns(_fixedTimestamp);
        _userProviderMock.Setup(p => p.GetCurrentUserId()).Returns("test-user");

        var services = new ServiceCollection();
        services.AddSingleton(_loggerMock.Object);
        services.AddSingleton(_userProviderMock.Object);
        services.AddSingleton(_timestampProviderMock.Object);
        var rootProvider = services.BuildServiceProvider();

        var interceptor = new AuditSaveChangesInterceptor(
            rootProvider.GetRequiredService<IServiceScopeFactory>(),
            new AuditOptions());

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .AddInterceptors(interceptor)
            .Options;

        _dbContext = new TestDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task SaveChangesAsync_Created_ShouldLogCreatedEntry()
    {
        // Arrange
        var entity = new TestEntity { Name = "Widget", Description = "A widget", Price = 9.99m };
        _dbContext.TestEntities.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(entries =>
                    entries.Count == 1 &&
                    entries[0].Action == AuditAction.Created &&
                    entries[0].EntityType == "TestEntity" &&
                    entries[0].Timestamp == _fixedTimestamp &&
                    entries[0].UserId == "test-user" &&
                    entries[0].Changes.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_Updated_ShouldLogOnlyChangedProperties()
    {
        // Arrange
        var entity = new TestEntity { Name = "Original", Description = "Desc", Price = 5.00m };
        _dbContext.TestEntities.Add(entity);
        await _dbContext.SaveChangesAsync();
        _loggerMock.Reset();
        _loggerMock.Setup(l => l.LogAsync(It.IsAny<IReadOnlyList<AuditEntry>>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        // Act
        entity.Name = "Updated";
        entity.Price = 10.00m;
        // Description stays the same
        await _dbContext.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(entries =>
                    entries.Count == 1 &&
                    entries[0].Action == AuditAction.Updated &&
                    entries[0].Changes.Count == 2 &&
                    entries[0].Changes.Any(c => c.PropertyName == "Name" && c.From == "Original" && c.To == "Updated") &&
                    entries[0].Changes.Any(c => c.PropertyName == "Price")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_Deleted_ShouldLogDeletedEntry()
    {
        // Arrange
        var entity = new TestEntity { Name = "ToDelete", Description = "Bye", Price = 1.00m };
        _dbContext.TestEntities.Add(entity);
        await _dbContext.SaveChangesAsync();
        _loggerMock.Reset();
        _loggerMock.Setup(l => l.LogAsync(It.IsAny<IReadOnlyList<AuditEntry>>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        // Act
        _dbContext.TestEntities.Remove(entity);
        await _dbContext.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(entries =>
                    entries.Count == 1 &&
                    entries[0].Action == AuditAction.Deleted &&
                    entries[0].EntityType == "TestEntity" &&
                    entries[0].Changes.Count == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_NoChanges_ShouldNotCallLogger()
    {
        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(It.IsAny<IReadOnlyList<AuditEntry>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleEntities_ShouldLogAll()
    {
        // Arrange
        _dbContext.TestEntities.Add(new TestEntity { Name = "A", Description = "D1", Price = 1.0m });
        _dbContext.TestEntities.Add(new TestEntity { Name = "B", Description = "D2", Price = 2.0m });

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(entries => entries.Count == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_Created_ShouldCaptureTemporaryKeyAsEntityId()
    {
        // Arrange — TestEntity has auto-increment int PK, so pre-save ID is a temporary negative value
        var entity = new TestEntity { Name = "New", Description = "Desc", Price = 1.0m };
        _dbContext.TestEntities.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert — EntityId is the EF Core temporary key (negative int), not the DB-assigned value
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(entries =>
                    entries.Count == 1 &&
                    entries[0].EntityId != null &&
                    entries[0].EntityId.Length > 0 &&
                    entries[0].EntityId.StartsWith("-")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_CompositeKey_ShouldJoinKeyValuesWithSemicolon()
    {
        // Arrange
        var entity = new CompositeKeyEntity { TenantId = "tenant-1", Code = "ABC", Name = "Test" };
        _dbContext.CompositeKeyEntities.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(entries =>
                    entries.Count == 1 &&
                    entries[0].EntityId == "tenant-1;ABC" &&
                    entries[0].Action == AuditAction.Created),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
