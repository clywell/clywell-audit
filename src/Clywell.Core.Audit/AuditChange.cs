namespace Clywell.Core.Audit;

/// <summary>
/// Represents a single property change within an audited entity update.
/// </summary>
/// <param name="PropertyName">The name of the property that changed.</param>
/// <param name="From">The original value before the change (serialized as string).</param>
/// <param name="To">The new value after the change (serialized as string).</param>
public sealed record AuditChange(string PropertyName, string? From, string? To);
