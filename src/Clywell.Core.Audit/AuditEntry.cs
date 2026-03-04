namespace Clywell.Core.Audit;

/// <summary>
/// Represents a single audit trail entry capturing an action performed on an entity.
/// </summary>
public sealed record AuditEntry
{
    /// <summary>Gets the unique identifier for this audit entry.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Gets the action that was performed.</summary>
    public required AuditAction Action { get; init; }

    /// <summary>Gets the CLR type name of the audited entity.</summary>
    public required string EntityType { get; init; }

    /// <summary>Gets the primary key of the audited entity (serialized as string).</summary>
    public required string EntityId { get; init; }

    /// <summary>Gets the UTC timestamp when the action occurred.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Gets an optional description of the action.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the identifier of the user who performed the action.</summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the list of property changes. Populated only for <see cref="AuditAction.Updated"/> actions
    /// (delta-only — only properties that actually changed are included).
    /// Empty for <see cref="AuditAction.Created"/>, <see cref="AuditAction.Deleted"/>, and <see cref="AuditAction.Read"/> actions.
    /// </summary>
    public IReadOnlyList<AuditChange> Changes { get; init; } = [];
}
