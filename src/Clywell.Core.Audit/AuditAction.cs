namespace Clywell.Core.Audit;

/// <summary>
/// The type of action that triggered an audit entry.
/// </summary>
public enum AuditAction
{
    /// <summary>A new entity was created.</summary>
    Created,

    /// <summary>An existing entity was updated.</summary>
    Updated,

    /// <summary>An entity was deleted.</summary>
    Deleted,

    /// <summary>An entity was read/accessed.</summary>
    Read
}
