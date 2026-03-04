using Microsoft.Extensions.DependencyInjection;

namespace Clywell.Core.Audit.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAudit_RegistersAuditService()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IAuditLogger>());

        services.AddAudit();

        var provider = services.BuildServiceProvider();
        var auditService = provider.GetService<IAuditService>();

        Assert.NotNull(auditService);
    }

    [Fact]
    public void AddAudit_RegistersTimestampProvider()
    {
        var services = new ServiceCollection();
        services.AddAudit();

        var provider = services.BuildServiceProvider();
        var timestampProvider = provider.GetService<IAuditTimestampProvider>();

        Assert.NotNull(timestampProvider);
    }

    [Fact]
    public void AddAudit_RegistersAuditOptionsAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddAudit(options => options.AuditOnly<ServiceCollectionExtensionsTests>());

        var provider = services.BuildServiceProvider();
        var options = provider.GetService<AuditOptions>();

        Assert.NotNull(options);
        Assert.True(options!.ShouldAudit(typeof(ServiceCollectionExtensionsTests)));
    }

    [Fact]
    public void AddAudit_WithoutConfigure_AuditsAll()
    {
        var services = new ServiceCollection();
        services.AddAudit();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<AuditOptions>();

        Assert.True(options.ShouldAudit(typeof(object)));
    }

    [Fact]
    public void AddAudit_NullServices_ShouldThrow()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.AddAudit());
    }

    [Fact]
    public void AddAudit_DoesNotOverwriteExistingTimestampProvider()
    {
        var services = new ServiceCollection();
        var customProvider = Mock.Of<IAuditTimestampProvider>();
        services.AddSingleton(customProvider);

        services.AddAudit();

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IAuditTimestampProvider>();

        Assert.Same(customProvider, resolved);
    }
}
