namespace Clywell.Core.Audit;

/// <summary>
/// Configuration options for the audit system.
/// </summary>
/// <remarks>
/// <para>
/// By default, all entities are audited. Use <see cref="AuditOnly{T}"/> to restrict
/// auditing to specific entity types, or <see cref="Ignore{T}"/> to exclude specific types.
/// </para>
/// <para>
/// <see cref="AuditOnly{T}"/> and <see cref="Ignore{T}"/> are mutually exclusive —
/// calling one mode after the other throws <see cref="InvalidOperationException"/>.
/// </para>
/// </remarks>
public sealed class AuditOptions
{
    private readonly HashSet<Type> _includedTypes = [];
    private readonly HashSet<Type> _excludedTypes = [];
    private AuditFilterMode _filterMode = AuditFilterMode.AuditAll;

    /// <summary>Gets the current filter mode.</summary>
    internal AuditFilterMode FilterMode => _filterMode;

    /// <summary>Gets the set of explicitly included types (when using <see cref="AuditFilterMode.IncludeOnly"/>).</summary>
    internal IReadOnlySet<Type> IncludedTypes => _includedTypes;

    /// <summary>Gets the set of explicitly excluded types (when using <see cref="AuditFilterMode.ExcludeOnly"/>).</summary>
    internal IReadOnlySet<Type> ExcludedTypes => _excludedTypes;

    /// <summary>
    /// Restricts auditing to only the specified entity type. Call multiple times to include
    /// multiple types. Cannot be combined with <see cref="Ignore{T}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type to audit.</typeparam>
    /// <returns>This instance for chaining.</returns>
    public AuditOptions AuditOnly<T>() where T : class
    {
        if (_filterMode == AuditFilterMode.ExcludeOnly)
            throw new InvalidOperationException("Cannot mix AuditOnly<T>() with Ignore<T>(). Use one or the other.");

        _filterMode = AuditFilterMode.IncludeOnly;
        _includedTypes.Add(typeof(T));
        return this;
    }

    /// <summary>
    /// Excludes the specified entity type from auditing. Call multiple times to exclude
    /// multiple types. Cannot be combined with <see cref="AuditOnly{T}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type to exclude from auditing.</typeparam>
    /// <returns>This instance for chaining.</returns>
    public AuditOptions Ignore<T>() where T : class
    {
        if (_filterMode == AuditFilterMode.IncludeOnly)
            throw new InvalidOperationException("Cannot mix Ignore<T>() with AuditOnly<T>(). Use one or the other.");

        _filterMode = AuditFilterMode.ExcludeOnly;
        _excludedTypes.Add(typeof(T));
        return this;
    }

    /// <summary>
    /// Determines whether the specified entity type should be audited based on the current configuration.
    /// </summary>
    /// <param name="entityType">The CLR type of the entity.</param>
    /// <returns><see langword="true"/> if the entity should be audited; otherwise <see langword="false"/>.</returns>
    public bool ShouldAudit(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        return _filterMode switch
        {
            AuditFilterMode.AuditAll => true,
            AuditFilterMode.IncludeOnly => _includedTypes.Contains(entityType),
            AuditFilterMode.ExcludeOnly => !_excludedTypes.Contains(entityType),
            _ => true
        };
    }
}

/// <summary>
/// Determines how entity types are filtered for auditing.
/// </summary>
internal enum AuditFilterMode
{
    /// <summary>All entities are audited (default).</summary>
    AuditAll,

    /// <summary>Only explicitly included entity types are audited.</summary>
    IncludeOnly,

    /// <summary>All entities except explicitly excluded types are audited.</summary>
    ExcludeOnly
}
