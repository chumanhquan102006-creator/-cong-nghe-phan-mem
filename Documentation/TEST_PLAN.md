# Test Plan

## Testing Objectives

- Verify that Academic AI Assistant meets the main functional requirements.
- Confirm that each user can only access their own documents, essays, checks, graph data, and chat history.
- Validate important error cases such as invalid PDF upload, missing extracted text, empty essay content, and unauthorized access.
- Ensure the UI is understandable and stable enough for classroom demo.

## Testing Scope

In scope:

- Authentication
- Dashboard
- Document management
- PDF processing
- Summary generation
- Writing Studio
- Writing Feedback
- Citation Checker
- Similarity Checker
- Knowledge Graph
- Chat with PDF
- Basic security and authorization

Out of scope:

- Real plagiarism checking across the Internet
- Production-grade AI accuracy evaluation
- Load testing
- Payment, quota, or admin management

## Test Environment

- OS: Windows
- IDE: Visual Studio / VS Code
- Framework: ASP.NET Core MVC
- Database: SQL Server LocalDB
- Browser: Chrome / Edge
- Runtime: .NET SDK

## Testing Strategy

- Automated unit testing for important rule-based service logic using xUnit.
- Manual functional testing using browser workflows.
- Negative testing with invalid inputs and unauthorized access.
- Regression testing after UI or controller changes.
- Database verification through expected screen output and dashboard counters.
- Rule-based AI fallback testing with `AI:Enabled` set to `false`.

## Automated Unit Testing

Automated tests are stored in the separate xUnit project:

```text
AcademicAIAssistant.Tests
```

Run automated tests from the repository parent folder:

```powershell
dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj
```

Automated unit tests currently cover:

- `FileValidationService`: valid and invalid PDF/JPG/PNG upload validation.
- `DocumentSummaryService`: short text handling and first-sentence summary generation.
- `WritingFeedbackService`: word count, citation, and informal tone feedback.
- `CitationCheckerService`: valid APA checks, missing references, unused references, and invalid years.
- `ReferenceGeneratorService`: APA/MLA reference and in-text citation generation.
- `LoginRateLimiter`: failed login counting, lockout, and reset behavior.
- `TextScanService`: internal similarity scanning using EF Core InMemory instead of SQL Server.

These tests do not call Gemini/OpenAI, do not use real SQL Server, and do not require uploaded files.

The following areas still require manual or integration testing:

- Razor views and browser navigation.
- Authentication cookie/session behavior end to end.
- Real PDF extraction with uploaded documents.
- OCR with local Tesseract traineddata.
- Real AI provider calls and quota/API key errors.
- JavaScript graph rendering and responsive UI.

## Functional Testing

Functional tests cover normal and error workflows for login, upload, extraction, summary, writing analysis, citation checking, similarity checking, graph building, and document chat.

## Security Testing

- Confirm unauthenticated users are redirected to Login for protected pages.
- Confirm one user cannot access another user's document, essay, citation result, similarity result, graph data, or chat history.
- Confirm uploaded filenames are stored safely using generated server filenames.
- Confirm real API keys are not committed to source code.

## Usability Testing

- Check that navigation links are clear.
- Check that alerts appear after important actions.
- Check that empty states explain what the user should do next.
- Check that detail pages have back buttons.
- Check that main demo flow can be completed on a laptop screen.

## Regression Testing

Run the main flow after each major change:

1. Register/Login.
2. Upload PDF.
3. Extract text.
4. Generate summary.
5. Chat with PDF.
6. Analyze essay.
7. Check citation.
8. Check similarity.
9. Build knowledge graph.
10. Confirm Dashboard updates.

## Acceptance Criteria

- User can register, login, and logout.
- Protected pages are not accessible without login.
- User can upload, view, process, and delete their own PDFs.
- User can analyze essays and view feedback history.
- Citation and similarity checks produce visible result pages.
- Knowledge Graph returns user-specific nodes and edges.
- Chat with PDF stores question and answer history.
- Dashboard statistics and recent activities reflect user actions.
- App runs with AI disabled using rule-based fallback mode.

## Risks and Limitations

- PDF extraction works best for text-based PDFs; scanned image PDFs may not extract readable text.
- Rule-based feedback and citation checks are simplified for academic demo purposes.
- Similarity Checker is internal only; it does not compare against Internet sources.
- Knowledge Graph keyword extraction is frequency-based, not semantic AI.
- AI integration is optional and disabled by default.
