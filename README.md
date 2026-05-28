# Academic AI Assistant

## Short Description

Academic AI Assistant is an ASP.NET Core MVC web application that helps students manage research PDFs, extract text, generate summaries, analyze academic writing, check citations, compare internal similarity, view a knowledge graph, and chat with uploaded PDFs.

## Problem Statement

Students often need to read many research papers and write academic essays, but the workflow is fragmented across PDF readers, writing tools, citation tools, and manual notes. This project provides one simple academic workspace for reading support and writing support.

## Main Features

- User registration, login, logout
- Dashboard with statistics and recent activities
- PDF upload and document library
- PDF text extraction
- Rule-based or AI-ready summary generation
- Writing Studio and academic feedback
- Citation Checker for basic APA checks
- Internal Similarity Checker against uploaded extracted PDFs
- Knowledge Graph for documents, essays, and keywords
- Chat with PDF using extracted document text
- AIService structure with rule-based fallback mode

## Tech Stack

- C# ASP.NET Core MVC
- Entity Framework Core
- SQL Server LocalDB
- HTML, CSS, JavaScript
- Bootstrap
- PdfPig for PDF text extraction

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
- AIService: OpenAI-compatible service structure, disabled by default

## How to Run Project

```powershell
cd "D:\viết luận\AcademicAIAssistant"
dotnet restore
dotnet build
dotnet run --launch-profile http
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

```powershell
dotnet ef database update
```

## How to Test Main Flow

1. Register a new account.
2. Login and open Dashboard.
3. Upload a PDF in Documents.
4. Extract text and generate summary.
5. Chat with the PDF.
6. Analyze an essay in Writing Studio.
7. Run Citation Checker.
8. Run Similarity Checker.
9. Build Knowledge Graph.
10. Return to Dashboard and check recent activities.

## Demo Account

No default demo account is seeded. Create one from:

```text
/Account/Register
```

Suggested demo account:

- Email: `demo@student.local`
- Password: `Demo@123`

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
