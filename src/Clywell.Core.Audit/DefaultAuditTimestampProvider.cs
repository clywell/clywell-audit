namespace Clywell.Core.Audit;


/// <summary>
/// Default implementation that returns <see cref="DateTimeOffset.UtcNow"/>.
/// </summary>
internal sealed class DefaultAuditTimestampProvider : IAuditTimestampProvider
{
    public DateTimeOffset GetCurrentTimestamp() => DateTimeOffset.UtcNow;
}