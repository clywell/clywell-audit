namespace Clywell.Core.Audit;

/// <summary>
/// Extension methods for registering Clywell.Core.Audit services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers audit services: <see cref="IAuditService"/> (scoped),
    /// <see cref="IAuditTimestampProvider"/> (singleton, defaults to UTC now).
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Optional delegate to configure <see cref="AuditOptions"/>.
    /// <code>
    /// services.AddAudit(options =&gt;
    /// {
    ///     options.AuditOnly&lt;Order&gt;();
    ///     options.AuditOnly&lt;Product&gt;();
    /// });
    /// </code>
    /// When omitted, all entities are audited.
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Consumers must register their own <see cref="IAuditLogger"/> implementation to
    /// define where audit records are persisted. An <see cref="IAuditUserProvider"/>
    /// registration is optional but recommended to stamp user identity on audit entries.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddAudit(this IServiceCollection services, Action<AuditOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new AuditOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.TryAddSingleton<IAuditTimestampProvider, DefaultAuditTimestampProvider>();
        services.TryAddScoped<IAuditService, AuditService>();

        return services;
    }
}
