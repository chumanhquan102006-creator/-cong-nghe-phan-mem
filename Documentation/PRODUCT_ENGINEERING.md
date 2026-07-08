# Product Engineering Notes

## Product Vision

Academic AI Assistant provides one academic workspace where students can read research documents, organize evidence, and improve academic writing. The product combines deterministic rule-based tools with optional Gemini-assisted workflows so the core application remains usable when AI is disabled or unavailable.

## v0.2.0 Product Evolution

The product direction changed from:

```text
Toolbox -> Workflow -> Product
```

`v0.1.0` delivered a broad academic toolbox. `v0.2.0` improves the product by organizing the document experience around a student workflow: upload, extract, summarize, and ask grounded questions in one Document Workspace.

This evolution was incremental:

```text
P0 cleanup -> navigation -> workspace -> polish
```

Evidence:

- `d72137b fix(ux): clean up critical product friction`
- `1395732 feat(ui): simplify navigation and information architecture`
- `a00f7d3 wip(documents): preserve document workspace progress`
- `fc8e8e0 fix(documents): finish document workspace polish`

## Target Users

- Students who need to read and summarize academic PDFs.
- Students who need structured feedback on essays and research writing.
- Students who need basic citation, reference, and internal similarity support.
- Student teams demonstrating an end-to-end software engineering process.

## Core Scenarios

1. Register, sign in, and manage a private academic workspace.
2. Upload a PDF, extract text, and generate a summary.
3. Ask questions using the extracted content of a document.
4. Analyze an essay and receive writing feedback.
5. Check basic APA citations and references.
6. Compare an essay with the user's extracted documents for internal similarity.
7. Extract text from an image through OCR.
8. Build a knowledge graph from documents, essays, and keywords.
9. Generate and manage APA/MLA references.
10. Configure an optional Gemini provider while retaining rule-based fallback behavior.

## Architecture Overview

- **Presentation layer:** ASP.NET Core MVC controllers and Razor views with Bootstrap and VI/EN localization.
- **Application layer:** services encapsulate PDF extraction, summaries, writing feedback, citation checks, similarity checks, OCR, document chat, knowledge graph construction, reference generation, and AI-provider calls.
- **Data layer:** Entity Framework Core stores users, documents, essays, scans, references, chat messages, graph data, and analysis results in SQL Server/LocalDB.
- **External integration:** Gemini is optional. Provider failures, missing keys, disabled settings, and timeouts fall back to deterministic rule-based services where supported.

## Product Qualities

### Usability

- Consistent Bootstrap layout, empty states, loading indicators, validation messages, and date formatting.
- Vietnamese and English UI resources for key demonstration workflows.
- Dashboard quick actions and recent activities make common tasks discoverable.

### Reliability

- Rule-based fallback keeps core workflows available without an AI connection.
- File validation checks extension, size, content type, and basic file signatures.
- Automated tests cover deterministic services without calling external APIs.
- `v0.2.0` improves reliable programming through OCR empty-state protection, TempData string serialization, short-question validation, and state-aware document actions.

### Maintainability

- MVC separates controllers, views, models, and data access.
- Business rules are implemented in services instead of Razor views.
- Shared localization resources and reusable UI partials reduce duplication.
- A solution file provides one repeatable restore/build/test entry point.
- `DocumentWorkspaceViewModel` keeps derived UI state out of controllers and views.
- Shared partials such as `Views/DocumentChat/_ChatPanel.cshtml` reduce duplicate UI behavior.
- Service boundaries keep PDF extraction, OCR, chat, summary, writing, citation, reference, similarity, and graph rules testable.

### Security and Privacy

- Cookie authentication protects private routes.
- Queries filter records using the authenticated user's identifier.
- Passwords are hashed and are never displayed.
- API keys are not committed and should not be logged.
- Uploaded user files, local databases, build output, and secrets are excluded from Git.
- The current API-key storage approach is suitable only for the academic demo; production use requires encryption and stronger secret management.

## Testing Strategy

- xUnit tests verify file validation, summary generation, writing feedback, citations, references, rate limiting, and internal text similarity.
- EF Core InMemory is used where data access is required, avoiding a real SQL Server dependency during unit tests.
- Tests do not call Gemini or other Internet services.
- GitHub Actions restores, builds, tests, publishes, and uploads a reproducible application artifact.
- Browser workflows, real OCR files, real PDFs, LocalDB migrations, and external AI calls remain manual acceptance-test areas.
- `v0.2.0` release-prep verification recorded 46 automated tests passing with 0 failed and 0 skipped.

## Release and Traceability

- Feature work is developed on branches and reviewed through GitHub pull requests.
- PR #27 consolidated profile, dashboard, empty-state, loading, date-format, localization, and validation improvements.
- `CHANGELOG.md` records the scope and known limitations of `v0.1.0`.
- `CHANGELOG.md` records the scope and known limitations of `v0.2.0`.
- CI run history and publish artifacts provide verifiable build/test evidence for release commits.
- `Documentation/RELEASE_NOTES_v0.2.0.md`, `Documentation/TEST_REPORT_v0.2.0.md`, `Documentation/ARCHITECTURE.md`, and `Documentation/SECURITY_AND_PRIVACY.md` provide theory-to-practice traceability for the release.

## DevOps

- GitHub Actions restores, builds in Release mode, runs tests, publishes the app, and uploads a workflow artifact.
- Release work is prepared on `release/v0.2.0` before PR and squash merge.
- The release artifact name is maintained as `academic-ai-assistant-release` instead of being tied to a stale version string.

## Risks and Limitations

- Similarity checking compares only internal user content and is not an Internet plagiarism detector.
- Citation checks are rule-based and do not guarantee full APA/MLA compliance.
- OCR accuracy depends on image quality and installed Tesseract language data.
- AI output may contain errors and must be reviewed by the student.
- LocalDB is a development database and is not a production deployment architecture.
- Cloud deployment, production monitoring, distributed storage, payment, and quota management are out of scope for `v0.2.0`.
- AI retrieval and source mapping can still be improved.
- Browser automation is not yet part of the automated CI suite.

## Future Improvements

- Encrypt user-supplied API keys and introduce production-grade secret management.
- Add integration and browser automation tests for critical user journeys.
- Add deployment configuration for a hosted SQL database and managed file storage.
- Improve citation parsing, similarity algorithms, and document retrieval.
- Add accessibility testing, observability, backup, and recovery procedures.
