namespace Clywell.Core.Audit.EntityFramework.Tests;

public class AuditOptionsFilterTests : IDisposable
{
    private readonly Mock<IAuditLogger> _loggerMock = new();
    private readonly TestDbContext _dbContext;

    public AuditOptionsFilterTests()
    {
        var auditOptions = new AuditOptions();
        auditOptions.AuditOnly<TestEntity>(); // Only audit TestEntity, not IgnoredEntity

        var services = new ServiceCollection();
        services.AddSingleton(_loggerMock.Object);
        services.AddSingleton(Mock.Of<IAuditTimestampProvider>());
        var rootProvider = services.BuildServiceProvider();

        var interceptor = new AuditSaveChangesInterceptor(
            rootProvider.GetRequiredService<IServiceScopeFactory>(),
            auditOptions);

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
    public async Task SaveChangesAsync_IncludedEntity_ShouldBeAudited()
    {
        // Arrange
        _dbContext.TestEntities.Add(new TestEntity { Name = "Audited", Description = "Yes", Price = 5.0m });

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(entries =>
                    entries.Count == 1 && entries[0].EntityType == "TestEntity"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_ExcludedEntity_ShouldNotBeAudited()
    {
        // Arrange
        _dbContext.IgnoredEntities.Add(new IgnoredEntity { Value = "Not audited" });

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(It.IsAny<IReadOnlyList<AuditEntry>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_MixedEntities_ShouldOnlyAuditIncluded()
    {
        // Arrange
        _dbContext.TestEntities.Add(new TestEntity { Name = "Tracked", Description = "D", Price = 1.0m });
        _dbContext.IgnoredEntities.Add(new IgnoredEntity { Value = "Ignored" });

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            l => l.LogAsync(
                It.Is<IReadOnlyList<AuditEntry>>(entries =>
                    entries.Count == 1 && entries[0].EntityType == "TestEntity"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
