# Security and Privacy

This document records repository evidence for the academic demo. It does not claim production readiness.

## Authentication

The application uses ASP.NET Core cookie authentication configured in `Program.cs`:

- Login path: `/Account/Login`
- Logout path: `/Account/Logout`
- Access denied path: `/Home/AccessDenied`
- Cookie lifetime: 7 days

Private controllers use `[Authorize]`, including document, dashboard, OCR, writing, citation, similarity, reference, settings, graph, and writing-coach flows.

## Authorization / Ownership

User records are scoped by the authenticated user id from `ClaimTypes.NameIdentifier`.

Repository evidence includes user-id filtering in:

- `DocumentsController`
- `DocumentChatController`
- `DashboardController`
- `OCRController`
- `WritingController`
- `TextScanController`
- `CitationController`
- `SimilarityController`
- `SettingsController`
- `GraphController`
- `WritingCoachController`

The data models include `UserId` fields for user-owned records such as documents, essays, OCR scans, references, chat messages, graph nodes/edges, and writing coach sessions.

## Password Handling

Account flows use password hashing and do not display stored passwords. Passwords should never be logged or stored in plain text. Change-password support is part of the authenticated account workflow.

## Secrets

`appsettings.json` contains an empty AI API key:

```json
"ApiKey": ""
```

The README instructs local developers to use User Secrets or environment variables for real keys. `.gitignore` excludes `secrets.json`, `appsettings.Development.json`, user files, logs, local databases, and temporary outputs.

No real API key was found in the audited configuration files.

## Uploaded Files

Uploaded files are local academic-demo storage. `.gitignore` excludes `wwwroot/uploads/**` while allowing `.gitkeep` placeholders.

The app includes upload validation through `FileValidationService` and tests for accepted file rules. Production use would require stronger scanning, storage isolation, quotas, retention policy, and malware protection.

## Development-Only Demo Tools

Development demo seed/reset tools are configured through `SeedData` settings and development-only startup behavior. `DevToolsController` is protected by authentication, and the README warns that demo credentials are for classroom/demo use only.

Risk: demo reset tools are useful for reproducible classroom demos but should not be exposed in production.

Safeguards:

- Seed/reset behavior is documented as development/demo only.
- `SeedData.AllowReset` can disable the reset button.
- Non-development startup does not run the development seeding block in `Program.cs`.

## Threats / Limitations

- LocalDB is development-only.
- API key storage needs stronger production handling.
- Uploaded file scanning is limited.
- No production-grade monitoring or alerting is present.
- No documented backup, retention, or incident response process is present.
- External AI output can be wrong and should be reviewed by students.
- Citation and similarity features are academic aids, not compliance guarantees.
