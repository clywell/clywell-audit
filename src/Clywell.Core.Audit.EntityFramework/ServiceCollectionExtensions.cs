namespace Clywell.Core.Audit.EntityFramework;

/// <summary>
/// Extension methods for registering Clywell.Core.Audit EF Core integration services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="AuditSaveChangesInterceptor"/> as a singleton service.
    /// Use <see cref="DbContextOptionsBuilderExtensions.UseAuditInterceptor"/> on your
    /// <see cref="DbContextOptionsBuilder"/> to attach it to a specific <see cref="DbContext"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Call <c>AddAudit()</c> before calling this method to register base audit services
    /// and configure <see cref="AuditOptions"/>.
    /// </para>
    /// <example>
    /// <code>
    /// services.AddAudit(options =&gt; options.AuditOnly&lt;Order&gt;());
    /// services.AddAuditEntityFramework();
    ///
    /// services.AddDbContext&lt;AppDbContext&gt;((sp, options) =&gt;
    /// {
    ///     options.UseSqlServer(connectionString)
    ///            .UseAuditInterceptor(sp);
    /// });
    /// </code>
    /// </example>
    /// </remarks>
    public static IServiceCollection AddAuditEntityFramework(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<AuditSaveChangesInterceptor>();

        return services;
    }
}
