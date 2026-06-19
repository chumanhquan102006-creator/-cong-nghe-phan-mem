# v0.1.0 - Initial Academic Demo

## Summary

This is a pre-release for academic demonstration purposes.

Academic AI Assistant is an ASP.NET Core MVC web application that helps students read research documents and improve academic writing. It supports PDF processing, document Q&A, writing feedback, citation checking, internal similarity scanning, OCR, knowledge graph visualization, writing coaching, and reference management.

## New Features

- Authentication: Register, Login, Logout, Change Password.
- Dashboard with user statistics and recent activities.
- User Profile page.
- Document Library with PDF upload.
- PDF text extraction and summary generation.
- Chat with PDF.
- Writing Studio and writing feedback.
- APA citation checker.
- Internal similarity scan.
- Knowledge Graph.
- OCR Scanner.
- AI Writing Coach.
- Reference Manager.
- Settings page for AI provider/Gemini.
- Demo seed/reset tools.
- VI/EN localization.
- Empty states and loading indicators.
- Consistent date display.
- Localized validation messages.

## Release Engineering Evidence

- The repository contains an xUnit test project with 30 automated tests.
- GitHub Actions runs restore, Release build, test, and publish on pull requests and pushes to `main`.
- The CI workflow uploads the published application as a downloadable workflow artifact.
- Installation, migration, and test commands use portable repository-relative paths.
- `CHANGELOG.md`, product engineering notes, and an MIT License are included.

## Installation

```bash
git clone https://github.com/chumanhquan102006-creator/-cong-nghe-phan-mem.git AcademicAIAssistant
cd AcademicAIAssistant
dotnet restore AcademicAIAssistant.sln
dotnet ef database update --project AcademicAIAssistant.csproj
dotnet run --project AcademicAIAssistant.csproj
```

## Testing Evidence

```bash
dotnet restore AcademicAIAssistant.sln
dotnet build AcademicAIAssistant.sln --configuration Release --no-restore
dotnet test AcademicAIAssistant.Tests/AcademicAIAssistant.Tests.csproj --configuration Release --no-build
```

Current verified local result:

- Release build: passed with 0 warnings and 0 errors.
- Automated tests: 30/30 passed.
- Publish output: generated successfully.
- GitHub Actions CI: configured; the release tag must be rebuilt only after the PR is merged and CI is green.

## Traceability

- PR #27 contains the consolidated UI/UX polish work.
- The release-evidence PR adds the in-repository test project, CI workflow, artifact generation, portable documentation, product engineering notes, and license.
- CI runs and artifacts will provide evidence tied to the merged release commit.

## Known Issues

- This release is intended for academic demonstration and is not production-ready.
- Similarity checking is internal only, not Internet-based and not Turnitin.
- Citation checking is rule-based and does not guarantee complete academic correctness.
- AI output should be reviewed before use.
- Gemini API requires individual user configuration.
- OCR may require local Tesseract `tessdata` files.
- Some OCR, browser, LocalDB, and external AI edge cases require manual testing.
- Cloud deployment is out of scope for `v0.1.0`.

## Contributors

- [chumanhquan102006-creator](https://github.com/chumanhquan102006-creator)
