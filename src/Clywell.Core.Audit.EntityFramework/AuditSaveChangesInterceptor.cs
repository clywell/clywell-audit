namespace Clywell.Core.Audit.EntityFramework;

/// <summary>
/// EF Core <see cref="SaveChangesInterceptor"/> that automatically captures entity changes
/// from the <see cref="ChangeTracker"/> and delegates them to the registered <see cref="IAuditLogger"/>.
/// </summary>
/// <remarks>
/// <para>
/// Registered as a <b>singleton</b>. Scoped dependencies (<see cref="IAuditLogger"/>,
/// <see cref="IAuditUserProvider"/>) are resolved per-invocation via <see cref="IServiceScopeFactory"/>
/// to avoid captive dependency issues.
/// </para>
/// <para>
/// Intercepts <c>SaveChangesAsync</c> to inspect tracked entities
/// before they are persisted. For each entity that passes the <see cref="AuditOptions"/> filter:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="AuditAction.Created"/>: records entity type and ID, no property changes.</description></item>
///   <item><description><see cref="AuditAction.Updated"/>: records only the properties that actually changed (delta-only).</description></item>
///   <item><description><see cref="AuditAction.Deleted"/>: records entity type and ID, no property changes.</description></item>
/// </list>
/// <para>
/// <b>Note:</b> For entities with database-generated keys (identity columns), the <see cref="AuditEntry.EntityId"/>
/// captured during <c>Created</c> actions reflects the temporary key value (e.g., <c>"0"</c>) since
/// interception occurs before the database assigns the final value. If the actual generated key is needed,
/// subscribe to <c>SavedChangesAsync</c> or capture the ID in a post-save callback.
/// </para>
/// </remarks>
internal sealed class AuditSaveChangesInterceptor(
    IServiceScopeFactory scopeFactory,
    AuditOptions auditOptions) : SaveChangesInterceptor
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly AuditOptions _auditOptions = auditOptions ?? throw new ArgumentNullException(nameof(auditOptions));

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await CaptureChangesAsync(eventData.Context, cancellationToken).ConfigureAwait(false);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask CaptureChangesAsync(DbContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var auditLogger = scope.ServiceProvider.GetRequiredService<IAuditLogger>();
        var userProvider = scope.ServiceProvider.GetService<IAuditUserProvider>();
        var timestampProvider = scope.ServiceProvider.GetRequiredService<IAuditTimestampProvider>();

        var entries = BuildAuditEntries(context.ChangeTracker, userProvider, timestampProvider);

        if (entries.Count > 0)
        {
            await auditLogger.LogAsync(entries, cancellationToken).ConfigureAwait(false);
        }
    }

    private List<AuditEntry> BuildAuditEntries(
        ChangeTracker changeTracker,
        IAuditUserProvider? userProvider,
        IAuditTimestampProvider timestampProvider)
    {
        var auditEntries = new List<AuditEntry>();
        var timestamp = timestampProvider.GetCurrentTimestamp();
        var userId = userProvider?.GetCurrentUserId();

        foreach (var entry in changeTracker.Entries())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            var entityType = entry.Entity.GetType();

            if (!_auditOptions.ShouldAudit(entityType))
                continue;

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified => AuditAction.Updated,
                EntityState.Deleted => AuditAction.Deleted,
                _ => (AuditAction?)null
            };

            if (action is null)
                continue;

            var entityId = GetPrimaryKeyValue(entry);

            var auditEntry = new AuditEntry
            {
                Action = action.Value,
                EntityType = entityType.Name,
                EntityId = entityId,
                Timestamp = timestamp,
                UserId = userId,
                Changes = action == AuditAction.Updated ? GetChanges(entry) : []
            };

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        string? firstValue = null;
        List<string>? compositeValues = null;

        foreach (var property in entry.Properties)
        {
            if (!property.Metadata.IsPrimaryKey())
                continue;

            var value = property.CurrentValue?.ToString() ?? string.Empty;

            if (firstValue is null)
            {
                firstValue = value;
                continue;
            }

            // We have a second key — we're dealing with a composite key
            compositeValues ??= [firstValue];
            compositeValues.Add(value);
        }

        if (compositeValues is not null)
            return string.Join(";", compositeValues);

        return firstValue ?? string.Empty;
    }

    private static List<AuditChange> GetChanges(EntityEntry entry)
    {
        var changes = new List<AuditChange>();

        foreach (var property in entry.Properties)
        {
            if (!property.IsModified)
                continue;

            changes.Add(new AuditChange(
                property.Metadata.Name,
                property.OriginalValue?.ToString(),
                property.CurrentValue?.ToString()));
        }

        return changes;
    }
}
