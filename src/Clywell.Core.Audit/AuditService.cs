namespace Clywell.Core.Audit;

/// <summary>
/// Default implementation of <see cref="IAuditService"/> that enriches audit entries
/// with user identity and timestamps before delegating to <see cref="IAuditLogger"/>.
/// </summary>
internal sealed class AuditService(
    IAuditLogger auditLogger,
    IAuditUserProvider? userProvider = null,
    IAuditTimestampProvider? timestampProvider = null) : IAuditService
{
    private readonly IAuditLogger _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    private readonly IAuditUserProvider? _userProvider = userProvider;
    private readonly IAuditTimestampProvider _timestampProvider = timestampProvider ?? new DefaultAuditTimestampProvider();

    /// <inheritdoc />
    public Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return _auditLogger.LogAsync([Enrich(entry)], cancellationToken);
    }

    /// <inheritdoc />
    public Task LogAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var enriched = new List<AuditEntry>(entries.Count);
        foreach (var entry in entries)
        {
            enriched.Add(Enrich(entry));
        }

        return _auditLogger.LogAsync(enriched, cancellationToken);
    }

    private AuditEntry Enrich(AuditEntry entry)
    {
        return entry with
        {
            UserId = entry.UserId ?? _userProvider?.GetCurrentUserId(),
            Timestamp = entry.Timestamp == default ? _timestampProvider.GetCurrentTimestamp() : entry.Timestamp
        };
    }
}
