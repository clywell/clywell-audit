namespace Clywell.Core.Audit.EntityFramework.Tests;

// ============================================================
// Test Infrastructure — shared entities and DbContext for tests
// ============================================================

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class IgnoredEntity
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class CompositeKeyEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    public DbSet<IgnoredEntity> IgnoredEntities => Set<IgnoredEntity>();
    public DbSet<CompositeKeyEntity> CompositeKeyEntities => Set<CompositeKeyEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompositeKeyEntity>()
            .HasKey(e => new { e.TenantId, e.Code });
    }
}
