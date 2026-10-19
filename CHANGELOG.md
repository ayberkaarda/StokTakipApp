# Changelog

## 1.0.0 - 2026-10-19

- Moved the project from .NET Framework 4.8 to .NET 10 (`net10.0-windows` UI, `net10.0` libraries).
- Switched to a layered architecture: Domain, Application, Data and UI projects; the UI follows the MVP pattern (presenter + view interfaces).
- Moved data access to EF Core 10; removed `DatabaseHelper` and added repositories and `UnitOfWork`. The schema is created with migrations (instead of a `.bak` file).
- Moved the connection string and log settings to `appsettings.json`; daily file logging with Serilog (14 days).
- Added paging, a date range filter and product search to the order history.
- Order deletion is now a soft delete; the deleted quantity is returned to stock.
- Added concurrency protection with `RowVersion`.
- Added categories and a per-product low stock threshold.
- Added reports: best sellers, daily and monthly summaries.
- Added user sign-in, an initial setup screen and Admin/Operator roles; permissions are verified on every operation against the user's current record in the database.
- Passwords are stored with PBKDF2-SHA256; added a password policy and a 15-minute sign-in lockout after 5 failed attempts.
- Added user management and password change screens.
- Added an audit log: who did what and when, with old/new values and the difference. Visible to the admin only; records cannot be modified through the application.
- Added CSV export for the order history and the audit log (Excel-compatible, protected against formula injection).
- Wrote a threat model (`docs/THREAT_MODEL.md`, in Turkish).
- Added tests: unit (Application), integration on SQLite and permission matrix (Data), UI, and optional real SQL Server tests.
- Added a GitHub Actions workflow: build, test, vulnerable package and migration checks, and a single-file win-x64 release package on `v*` tags.
