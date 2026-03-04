namespace Clywell.Core.Audit.Tests;

public class AuditOptionsTests
{
    [Fact]
    public void ShouldAudit_Default_ReturnsTrue()
    {
        var options = new AuditOptions();

        Assert.True(options.ShouldAudit(typeof(TestEntity)));
    }

    [Fact]
    public void AuditOnly_IncludedType_ReturnsTrue()
    {
        var options = new AuditOptions();
        options.AuditOnly<TestEntity>();

        Assert.True(options.ShouldAudit(typeof(TestEntity)));
    }

    [Fact]
    public void AuditOnly_ExcludedType_ReturnsFalse()
    {
        var options = new AuditOptions();
        options.AuditOnly<TestEntity>();

        Assert.False(options.ShouldAudit(typeof(OtherEntity)));
    }

    [Fact]
    public void Ignore_ExcludedType_ReturnsFalse()
    {
        var options = new AuditOptions();
        options.Ignore<TestEntity>();

        Assert.False(options.ShouldAudit(typeof(TestEntity)));
    }

    [Fact]
    public void Ignore_NonExcludedType_ReturnsTrue()
    {
        var options = new AuditOptions();
        options.Ignore<TestEntity>();

        Assert.True(options.ShouldAudit(typeof(OtherEntity)));
    }

    [Fact]
    public void AuditOnly_ThenIgnore_ShouldThrow()
    {
        var options = new AuditOptions();
        options.AuditOnly<TestEntity>();

        Assert.Throws<InvalidOperationException>(() => options.Ignore<OtherEntity>());
    }

    [Fact]
    public void Ignore_ThenAuditOnly_ShouldThrow()
    {
        var options = new AuditOptions();
        options.Ignore<TestEntity>();

        Assert.Throws<InvalidOperationException>(() => options.AuditOnly<OtherEntity>());
    }

    [Fact]
    public void AuditOnly_MultipleTypes_IncludesAll()
    {
        var options = new AuditOptions();
        options.AuditOnly<TestEntity>();
        options.AuditOnly<OtherEntity>();

        Assert.True(options.ShouldAudit(typeof(TestEntity)));
        Assert.True(options.ShouldAudit(typeof(OtherEntity)));
        Assert.False(options.ShouldAudit(typeof(ThirdEntity)));
    }

    [Fact]
    public void AuditOnly_ReturnsSelf_ForChaining()
    {
        var options = new AuditOptions();
        var result = options.AuditOnly<TestEntity>();

        Assert.Same(options, result);
    }

    [Fact]
    public void Ignore_ReturnsSelf_ForChaining()
    {
        var options = new AuditOptions();
        var result = options.Ignore<TestEntity>();

        Assert.Same(options, result);
    }

    [Fact]
    public void ShouldAudit_NullType_ShouldThrow()
    {
        var options = new AuditOptions();

        Assert.Throws<ArgumentNullException>(() => options.ShouldAudit(null!));
    }

    // Test entity types
    private class TestEntity { }
    private class OtherEntity { }
    private class ThirdEntity { }
}
