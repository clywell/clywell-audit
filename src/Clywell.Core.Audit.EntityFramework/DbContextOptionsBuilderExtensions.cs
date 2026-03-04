namespace Clywell.Core.Audit.EntityFramework;

/// <summary>
/// Extension methods for attaching the audit interceptor to a <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="AuditSaveChangesInterceptor"/> to the <see cref="DbContext"/> options.
    /// Call <see cref="ServiceCollectionExtensions.AddAuditEntityFramework"/> first to register the interceptor.
    /// </summary>
    /// <param name="builder">The DbContext options builder.</param>
    /// <param name="serviceProvider">The service provider used to resolve the interceptor.</param>
    /// <returns>The options builder for chaining.</returns>
    public static DbContextOptionsBuilder UseAuditInterceptor(
        this DbContextOptionsBuilder builder,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var interceptor = serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>();
        builder.AddInterceptors(interceptor);

        return builder;
    }
}
