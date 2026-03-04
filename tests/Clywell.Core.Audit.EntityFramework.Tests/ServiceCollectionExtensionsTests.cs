namespace Clywell.Core.Audit.EntityFramework.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAuditEntityFramework_RegistersInterceptor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAudit();
        services.AddSingleton(new Mock<IAuditLogger>().Object);

        // Act
        services.AddAuditEntityFramework();

        // Assert
        var provider = services.BuildServiceProvider();
        var interceptor = provider.GetService<AuditSaveChangesInterceptor>();
        Assert.NotNull(interceptor);
    }

    [Fact]
    public void AddAuditEntityFramework_IsIdempotent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAudit();

        // Act
        services.AddAuditEntityFramework();
        services.AddAuditEntityFramework();

        // Assert
        var descriptors = services.Where(d => d.ServiceType == typeof(AuditSaveChangesInterceptor)).ToList();
        Assert.Single(descriptors);
    }

    [Fact]
    public void AddAuditEntityFramework_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAudit();

        // Act
        var result = services.AddAuditEntityFramework();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void UseAuditInterceptor_AddsInterceptorToOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAudit();
        services.AddSingleton(new Mock<IAuditLogger>().Object);
        services.AddAuditEntityFramework();
        var provider = services.BuildServiceProvider();

        // Act & Assert — should not throw
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .UseAuditInterceptor(provider)
            .Options;

        Assert.NotNull(options);
    }

    [Fact]
    public void UseAuditInterceptor_ThrowsWhenServiceProviderIsNull()
    {
        // Arrange
        var builder = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.UseAuditInterceptor(null!));
    }
}
