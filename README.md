# Academic AI Assistant

[![.NET CI](https://github.com/chumanhquan102006-creator/-cong-nghe-phan-mem/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/chumanhquan102006-creator/-cong-nghe-phan-mem/actions/workflows/dotnet-ci.yml)

## Current Release

v0.2.0 - Document Workspace & Product UX

This release is intended for course demonstration and is not production-ready.

## Product Evolution

Academic AI Assistant started as a feature-rich academic toolbox in `v0.1.0`: reading, writing, OCR, citation, similarity, references, and knowledge graph tools were available as separate capabilities.

In `v0.2.0`, the product direction moves toward a workflow-oriented academic product. The main document flow now guides a student from upload, to extraction, to summary, to grounded document questions in one Document Workspace.

## Major v0.2.0 Improvements

- Simplified navigation and information architecture.
- Document Workspace for document-centered reading work.
- Overview, Summary, and Ask AI tabs.
- State-aware document workflow for uploaded, extracted, and fully processed documents.
- Better Chat with PDF short-question handling.
- OCR empty/failure UX.
- Disabled invalid OCR actions when no readable text exists.
- Friendlier error and status messages.
- Copy, export, counters, and toast feedback.
- PDF text normalization improvements.
- Vietnamese and English product polish.
- Automated tests increased to 46.

## Short Description

Academic AI Assistant is an ASP.NET Core MVC web application that helps students manage research PDFs, extract text, generate summaries, analyze academic writing, check citations, compare internal similarity, view a knowledge graph, and chat with uploaded PDFs.

## Problem Statement

Students often need to read many research papers and write academic essays, but the workflow is fragmented across PDF readers, writing tools, citation tools, and manual notes. This project provides one simple academic workspace for reading support and writing support.

## Main Features

- User registration, login, logout
- Dashboard with statistics and recent activities
- PDF upload and document library
- Document Workspace with Overview / Summary / Ask AI tabs
- PDF text extraction
- Rule-based or AI-ready summary generation
- Writing Studio and academic feedback
- Citation Checker for basic APA checks
- Internal Similarity Checker against uploaded extracted PDFs
- Knowledge Graph for documents, essays, and keywords
- Chat with PDF using extracted document text
- OCR Scanner for image-to-text extraction
- AIService structure with rule-based fallback mode

## Tech Stack

- C# ASP.NET Core MVC
- Entity Framework Core
- SQL Server LocalDB
- HTML, CSS, JavaScript
- Bootstrap
- PdfPig for PDF text extraction
- Tesseract for local OCR image-to-text extraction

## System Modules

- Authentication: account registration, login, logout
- Dashboard: statistics, recent documents, recent essays, recent activities
- Documents: PDF upload, library, details, delete
- PDF Processing: extract text, word count, page count, summary
- Writing: essay analysis, feedback reports, writing history
- Citation: APA-style rule-based checks
- Similarity: internal text overlap comparison
- Knowledge Graph: document, essay, and keyword relationships
- Document Chat: rule-based retrieval and optional AI fallback answer
- OCR Scanner: upload JPG/PNG images and extract readable text
- AIService: OpenAI-compatible service structure, disabled by default

## How to Run Project

```bash
git clone https://github.com/chumanhquan102006-creator/-cong-nghe-phan-mem.git AcademicAIAssistant
cd AcademicAIAssistant
dotnet restore AcademicAIAssistant.sln
dotnet ef database update --project AcademicAIAssistant.csproj
dotnet build AcademicAIAssistant.sln
dotnet run --project AcademicAIAssistant.csproj --launch-profile http
```

Open the app:

```text
http://localhost:5010
```

## How to Apply Migrations

Install EF tool if needed:

```powershell
dotnet tool install --global dotnet-ef
```

Apply database migrations:

```bash
dotnet ef database update --project AcademicAIAssistant.csproj
```

## How to Test Main Flow

1. Register a new account.
2. Login and open Dashboard.
3. Upload a PDF in Documents.
4. Extract text and generate summary.
5. Chat with the PDF.
6. Upload an image in OCR Scanner and send extracted text to Writing Studio.
7. Analyze an essay in Writing Studio.
8. Run Citation Checker.
9. Run Similarity Checker.
10. Build Knowledge Graph.
11. Return to Dashboard and check recent activities.

## Testing

The repository includes an xUnit test project for important rule-based services. These tests do not require SQL Server, uploaded files, Internet access, or a real AI API key.

```bash
dotnet test AcademicAIAssistant.sln
```

Current verified local result:

- Debug test-project build: passed with 0 warnings and 0 errors
- Automated tests: 46/46 passed, 0 failed, 0 skipped

Automated coverage includes:

- File validation for PDF/JPG/PNG upload rules
- Rule-based document summary generation
- Rule-based writing feedback
- APA citation checking
- Reference generation
- Login rate limiter behavior
- Internal text similarity scanning with EF Core InMemory

Browser workflows, real PDF files, OCR traineddata, and real AI provider calls should still be tested manually.

## Continuous Integration

The repository includes a GitHub Actions workflow that runs on pull requests and pushes to `main`.

The workflow verifies:

- `dotnet restore AcademicAIAssistant.sln`
- Release build of `AcademicAIAssistant.sln`
- Automated xUnit tests from `AcademicAIAssistant.Tests`
- Publish of `AcademicAIAssistant.csproj`
- Upload of the published application as a workflow artifact

CI configuration: [`.github/workflows/dotnet-ci.yml`](.github/workflows/dotnet-ci.yml)

## OCR Setup

OCR uses local Tesseract language data from the project `tessdata` folder.

Required files:

```text
tessdata/eng.traineddata
tessdata/vie.traineddata
```

These traineddata files are not committed because they can be large. Download them from the official Tesseract language data repository, place them in `tessdata`, then restart the app.

## Demo Account

In Development, demo seed data is enabled by default. After applying migrations and running the app, you can login with:

- Email: `demo@student.com`
- Password: `Demo@123456`

The demo account is for classroom demo/development only. Do not use demo credentials in production.

Seed data includes a sample document, essay, feedback report, citation result, similarity result, OCR scan, writing coach sessions, references, chat history, and knowledge graph nodes/edges.

You can turn seed data on or off in `appsettings.json`:

```json
"SeedData": {
  "Enabled": true,
  "AllowReset": true
}
```

Set `Enabled` to `false` if you do not want the app to create demo data on startup.

In Development, logged-in users can open:

```text
/DevTools
```

Use **Reset Demo Data** to delete and recreate only the demo records for `demo@student.com`. This route is not available outside the Development environment. Set `SeedData.AllowReset` to `false` to disable the reset button.

## AI Mode Note

The app can run completely in rule-based fallback mode. AI API key is optional.

AI settings are stored in `appsettings.json`:

```json
"AI": {
  "Provider": "OpenAI",
  "ApiKey": "",
  "Endpoint": "https://api.openai.com/v1/chat/completions",
  "Model": "gpt-4o-mini",
  "Enabled": false,
  "TimeoutSeconds": 30
}
```

Do not commit a real API key. For local development, use User Secrets or environment variables:

```powershell
dotnet user-secrets set "AI:ApiKey" "YOUR_API_KEY"
dotnet user-secrets set "AI:Enabled" "true"
```

## Documentation

Project documents are in the `Documentation` folder:

- `TEST_PLAN.md`
- `TEST_CASES.md`
- `DEMO_FLOW.md`
- `PROJECT_SUMMARY.md`
- `FEATURES.md`
- `INSTALLATION_GUIDE.md`
- `PRODUCT_ENGINEERING.md`
- `RELEASE_NOTES_v0.1.0.md`
- `RELEASE_NOTES_v0.2.0.md`
- `TEST_REPORT_v0.2.0.md`
- `ARCHITECTURE.md`
- `SECURITY_AND_PRIVACY.md`
- `TEAM_CONTRIBUTIONS_v0.2.0.md`
- `RELEASE_CHECKLIST_v0.2.0.md`
- `EVIDENCE_INDEX_v0.2.0.md`

Also see [`SELF_ASSESSMENT.md`](SELF_ASSESSMENT.md) for the evidence-based rubric proposal.

Release history is documented in [`CHANGELOG.md`](CHANGELOG.md). The project is distributed under the [MIT License](LICENSE).
