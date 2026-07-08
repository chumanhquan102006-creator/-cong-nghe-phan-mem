# Architecture

## System Context

```text
Student
  -> ASP.NET Core MVC
  -> Controllers and Services
  -> EF Core / SQL Server LocalDB
  -> Optional Gemini-compatible AI provider
  -> Local PDF/OCR processing
```

Academic AI Assistant is an ASP.NET Core MVC academic demo. It stores user academic data in EF Core models, processes PDFs and OCR locally, and optionally calls an AI provider when configured. Rule-based services keep core flows usable when AI is disabled or unavailable.

## Layered Structure

### Presentation

- Controllers in `Controllers/`.
- Razor views in `Views/`.
- View models in `Models/ViewModels/`.
- Shared UI partials such as `Views/Shared/_Alerts.cshtml`, `Views/Shared/_EmptyState.cshtml`, `Views/References/_CopyBlock.cshtml`, and `Views/DocumentChat/_ChatPanel.cshtml`.
- Localization resources in `Resources/`.

### Application / Services

Services in `Services/` encapsulate business rules and integrations:

- PDF extraction: `PdfTextExtractionService`.
- Summary generation: `DocumentSummaryService` and optional `AIService`.
- Writing feedback: `WritingFeedbackService` and `WritingCoachFallbackService`.
- Citation/reference support: `CitationCheckerService`, `ReferenceGeneratorService`.
- Similarity/text scanning: `SimilarityCheckerService`, `TextScanService`.
- Document chat: `DocumentChatService`.
- OCR: `OCRService`.
- Knowledge graph: `KnowledgeGraphService`.
- Upload validation and login safety: `FileValidationService`, `LoginRateLimiter`.

### Data

- EF Core context: `Data/AppDbContext.cs`.
- Domain models in `Models/`.
- SQL Server LocalDB connection in `appsettings.json`.
- Existing migrations in `Migrations/`.

### External Integrations

- Optional Gemini-compatible AI settings in `appsettings.json` and per-user settings.
- Local Tesseract OCR with traineddata in `tessdata/`.
- PdfPig for local PDF text extraction.

## Key Decisions

### MVC Separation

Theory: separation of concerns.

Decision: route user actions through controllers, keep display logic in Razor views, keep business rules in services, and use EF Core models for persistence.

Evidence: `Controllers/`, `Views/`, `Services/`, `Models/`, `Data/AppDbContext.cs`.

### Rule-Based Fallback

Theory: fault tolerance and graceful degradation.

Decision: core learning workflows should still run without external AI credentials or network access.

Evidence: `DocumentSummaryService`, `WritingFeedbackService`, `WritingCoachFallbackService`, `DocumentChatService`, and `AIService` fallback/error handling.

### Document Workspace ViewModel

Theory: presentation model and reduced coupling.

Decision: derive workspace states from existing document fields instead of changing the database schema.

Evidence: `Models/ViewModels/DocumentWorkspaceViewModel.cs`, `Views/Documents/Details.cshtml`, and `AcademicAIAssistant.Tests/ViewModels/DocumentWorkspaceViewModelTests.cs`.

### Shared `_ChatPanel.cshtml`

Theory: reuse and maintainability.

Decision: reuse a chat panel partial for document chat UI instead of duplicating chat markup in each view.

Evidence: `Views/DocumentChat/_ChatPanel.cshtml` and the Document Workspace details view.

### State-Aware Actions

Theory: defensive programming and reliable UI.

Decision: show or disable actions based on extracted text, summary availability, and OCR result state.

Evidence: `DocumentsController`, `DocumentChatController`, `OCRController`, `TextScanController`, `WritingController`, and `Views/Documents/Details.cshtml`.

### Incremental Delivery

Theory: incremental software development and continuous validation.

Decision: improve the product through small steps:

```text
P0 cleanup -> Navigation -> Workspace -> Polish
```

Evidence:

- `d72137b fix(ux): clean up critical product friction`
- `1395732 feat(ui): simplify navigation and information architecture`
- `a00f7d3 wip(documents): preserve document workspace progress`
- `fc8e8e0 fix(documents): finish document workspace polish`

## Trade-Offs

- LocalDB and local uploads keep the academic demo simple but are not production deployment architecture.
- Rule-based fallback improves reliability but cannot match robust retrieval with source citations.
- Razor MVC is appropriate for course scope, but richer client-side state could improve advanced document workflows.
- User-scoped settings are implemented for the demo; production secret storage would require stronger protection.
- Manual browser verification remains important because automated coverage is mostly service and view-model level.
