namespace Clywell.Core.Audit;

/// <summary>
/// Provides the current UTC timestamp for audit entries. Override the default implementation
/// when deterministic timestamps are needed (e.g., in tests).
/// </summary>
public interface IAuditTimestampProvider
{
    /// <summary>Gets the current UTC timestamp.</summary>
    DateTimeOffset GetCurrentTimestamp();
}