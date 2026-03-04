# Changelog

All notable changes to Clywell.Core.Audit will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [1.0.0] - 2026-03-04

### Added
- Initial release of `Clywell.Core.Audit` with audit trail abstractions
- `AuditEntry` and `AuditChange` models
- `IAuditLogger`, `IAuditUserProvider`, `IAuditTimestampProvider` interfaces
- `IAuditService` with default `AuditService` implementation for manual audit logging
- `AuditOptions` with fluent configuration (`AuditOnly<T>()`, `Ignore<T>()`)
- `AddAudit()` DI extension method
- Initial release of `Clywell.Core.Audit.EntityFramework` with EF Core integration
- `AuditSaveChangesInterceptor` for automatic entity change tracking
- Delta-only change tracking (only changed properties recorded for updates)
- `AddAuditEntityFramework()` DI extension method
- `UseAuditInterceptor(IServiceProvider)` DbContext options extension method
