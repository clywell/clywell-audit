namespace Clywell.Core.Audit;

/// <summary>
/// Resolves the current user identity for audit entries. Consumers must implement
/// this interface to supply the user ID that is stamped on each <see cref="AuditEntry"/>.
/// </summary>
public interface IAuditUserProvider
{
    /// <summary>
    /// Gets the identifier of the current user, or <see langword="null"/> if unavailable.
    /// </summary>
    string? GetCurrentUserId();
}
