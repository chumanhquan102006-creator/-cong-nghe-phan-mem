# Changelog

## v0.2.0 - Document Workspace & Product UX

### Added

- Document Workspace for document-centered reading workflows.
- Overview, Summary, and Ask AI tabs on document details.
- `DocumentWorkspaceViewModel` for derived document workflow state without a database schema change.
- Reusable `Views/DocumentChat/_ChatPanel.cshtml` partial.
- New automated tests for document workspace state, PDF text normalization, document chat, and UX polish behavior.

### Changed

- Simplified navigation and reorganized information architecture.
- Improved document state actions for upload-only, extracted, and fully processed documents.
- Improved PDF text normalization before downstream summary/chat use.
- Improved localization consistency for Vietnamese and English product flows.
- Updated GitHub Actions publish artifact name to `academic-ai-assistant-release`.

### Fixed

- `LocalizedString` TempData serialization crash.
- Valid short questions rejected by Document Chat.
- OCR actions shown with an empty OCR result.
- Technical document metadata exposed in the UI.
- Writing Studio placeholder route.
- `1 Pages` pluralization issue.
- Duplicated processing-state information.
- Old demo chat response evidence.

### Testing

- Baseline verification on the release preparation branch:
  - `dotnet build`: failed during solution restore/build selection with 0 warnings and 0 errors reported.
  - `dotnet restore AcademicAIAssistant.sln -p:RestoreUseSkipNonexistentTargets=false`: passed.
  - `dotnet build AcademicAIAssistant.sln --configuration Release --no-restore`: passed with 1 MSBuild cache warning and 0 errors after sandbox-permission rerun.
  - `dotnet build AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --no-restore`: passed with 0 warnings and 0 errors.
  - `dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --no-restore --no-build`: 46 passed, 0 failed, 0 skipped.
  - `dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj`: 46 passed, 0 failed, 0 skipped.
  - `dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --configuration Release --no-build --verbosity normal`: 46 passed, 0 failed, 0 skipped.
  - `dotnet publish AcademicAIAssistant.csproj --configuration Release --no-build --output ./publish`: passed.

### Known Limitations

- This is an academic demo and is not production-ready.
- LocalDB and local file uploads are development/demo storage choices.
- External AI calls remain optional and require local user configuration.
- OCR quality depends on image quality and local Tesseract traineddata.
- Similarity and citation checks are rule-based learning aids, not authoritative academic compliance tools.
- Browser, OCR, and external AI workflows still require manual acceptance checks in addition to unit tests.

## v0.1.0 - Initial Release

### Added

- Authentication: Register, Login, Logout, Change Password.
- Dashboard with user statistics and recent activities.
- Document Library with PDF upload.
- Text extraction and summary generation.
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
- UI/UX polish:
  - Profile page
  - Empty states
  - Loading indicators
  - Consistent date format
  - Localization cleanup
  - Localized validation messages

### Notes

- This is an early `0.x` release for academic demo purposes.
- Behavior, configuration, and UI may change before `v1.0.0`.

### Release Engineering Evidence

- Added an automated xUnit test project to the repository.
- Added GitHub Actions CI for restore, Release build, test, and publish.
- Added CI artifact generation for the published application.
- Updated installation and test instructions to be portable.
- Added product engineering documentation.
- Added the MIT License.

### Test Evidence

- `dotnet restore AcademicAIAssistant.sln`: passed locally.
- `dotnet build AcademicAIAssistant.sln --configuration Release`: passed locally.
- `dotnet test AcademicAIAssistant.sln`: 30/30 tests passed locally.
- GitHub Actions CI is configured for pull requests and pushes to `main`.

### Known Limitations

- This release is intended for academic demonstration and is not production-ready.
- Similarity scan is internal only, not Internet-based and not Turnitin.
- Citation checking is basic and does not guarantee full academic correctness.
- AI output should be reviewed by students before use.
- Gemini API requires user configuration.
- Rule-based fallback logic is used when AI is disabled or unavailable.
- OCR may require local Tesseract `tessdata` files.
- Some browser, OCR, and external AI edge cases require manual testing.
