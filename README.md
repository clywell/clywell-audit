# Clywell.Core.Audit

Audit trail abstractions and EF Core implementation for .NET — entity change tracking, manual audit logging, and pluggable audit persistence.

## Installation

```bash
dotnet add package Clywell.Core.Audit
```

For automatic EF Core change tracking:

```bash
dotnet add package Clywell.Core.Audit.EntityFramework
```

## Packages

| Package | Description |
|---------|-------------|
| `Clywell.Core.Audit` | Audit abstractions — models, interfaces, and direct audit API. Zero infrastructure dependency. |
| `Clywell.Core.Audit.EntityFramework` | EF Core implementation — automatic change tracking via `SaveChangesInterceptor`. |

## Quick Start

### Manual Audit Logging

```csharp
services.AddAudit();
services.AddScoped<IAuditLogger, MyAuditLogger>();
```

Implement `IAuditLogger` to define where audit records are persisted:

```csharp
public class MyAuditLogger : IAuditLogger
{
    public Task LogAsync(IReadOnlyList<AuditEntry> entries, CancellationToken ct)
    {
        // Persist to database, queue, file, etc.
        return Task.CompletedTask;
    }
}
```

### Automatic EF Core Change Tracking

```csharp
services.AddAudit(options =>
{
    options.AuditOnly<Order>();
    options.AuditOnly<Product>();
});
services.AddScoped<IAuditLogger, MyAuditLogger>();
services.AddAuditEntityFramework();

services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString)
           .UseAuditInterceptor(sp);
});
```

## Notes

- `IAuditLogger` is required. Register your implementation in DI before writing audit records.
- `SaveChangesAsync` is required for EF interception. Synchronous `SaveChanges()` is intentionally not supported by `AuditSaveChangesInterceptor`.
- `AddAuditEntityFramework()` registers the interceptor; attach it to each DbContext with `UseAuditInterceptor(sp)`.

## Compatibility

- .NET target: `net10.0`
- EF integration dependency: `Microsoft.EntityFrameworkCore`

## Key Concepts

- **Delta-only tracking**: For `Updated` actions, only properties that actually changed are recorded.
- **Scoped resolution**: The interceptor is a singleton but resolves `IAuditLogger`, `IAuditUserProvider`, and `IAuditTimestampProvider` per-invocation — safe for scoped dependencies.
- **Database-generated keys**: For `Created` actions with identity columns, `EntityId` reflects the temporary key (e.g., `"0"`) since interception occurs before the database assigns the final value.

## License

MIT — see [LICENSE](LICENSE).
