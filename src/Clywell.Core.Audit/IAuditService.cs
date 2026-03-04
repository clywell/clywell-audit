namespace Clywell.Core.Audit;

/// <summary>
/// Direct API for manual audit logging. Use this when you need to record audit entries
/// outside of automatic EF Core change tracking (e.g., for read operations, business events,
/// or non-EF data stores).
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Records a single audit entry.
    /// </summary>
    /// <param name="entry">The audit entry to record.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records multiple audit entries in a single batch.
    /// </summary>
    /// <param name="entries">The audit entries to record.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task LogAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default);
}
