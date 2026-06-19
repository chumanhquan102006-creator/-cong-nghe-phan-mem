# Changelog

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
