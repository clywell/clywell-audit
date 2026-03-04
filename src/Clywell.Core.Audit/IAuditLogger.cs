namespace Clywell.Core.Audit;

/// <summary>
/// Persists audit entries to a backing store. Consumers must implement this interface
/// to define where audit records are stored (database, message queue, file, etc.).
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Persists a single audit entry.
    /// </summary>
    /// <param name="entry">The audit entry to persist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default) =>
        LogAsync([entry], cancellationToken);

    /// <summary>
    /// Persists a batch of audit entries.
    /// </summary>
    /// <param name="entries">The audit entries to persist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task LogAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default);
}
