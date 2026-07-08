# v0.2.0 - Document Workspace & Product UX

## Release Summary

Academic AI Assistant `v0.2.0` is an academic demo release focused on turning the document experience from separate actions into a coherent workspace. The release keeps the existing reading, writing, OCR, citation, similarity, reference, and knowledge graph capabilities while improving navigation, document state, localized messages, and verification evidence.

## Why This Release Matters

The project moved from a collection of independent tools toward a coherent user workflow. In `v0.1.0`, students could access many academic support tools, but the experience still felt like a toolbox. In `v0.2.0`, the document path is organized around upload, extraction, summary, and grounded questions in one workspace.

## Highlights

### Document Workspace

- Adds a document-centered workspace in `Views/Documents/Details.cshtml`.
- Introduces Overview, Summary, and Ask AI tabs.
- Uses `Models/ViewModels/DocumentWorkspaceViewModel.cs` to derive UI state from existing document fields.
- Reuses `Views/DocumentChat/_ChatPanel.cshtml` for chat behavior.

### Navigation and Information Architecture

- Simplifies the main navigation and reduces product friction.
- Moves users toward workflow destinations instead of exposing every tool with equal weight.
- Keeps advanced academic tools available while making the document workflow clearer.

### Reliability and Failure-State Handling

- Preserves rule-based fallback behavior when AI is disabled or unavailable.
- Improves short-question handling in Document Chat.
- Prevents invalid OCR follow-up actions when OCR returns no readable text.
- Serializes TempData messages as strings to avoid localized message crashes.

### Testing and Verification

- Automated test baseline verified locally at 46 passed, 0 failed, 0 skipped.
- Test areas include file validation, summaries, writing feedback, citations, references, login rate limiting, text similarity, PDF text normalization, document chat, and document workspace view-model state.
- Browser smoke testing, language checks, and responsive checks are recorded as manual acceptance evidence rather than automated screenshots.

### Localization and Usability

- Vietnamese and English product polish is retained and extended into the document workflow.
- Friendly messages, copy/export actions, counters, toasts, and better empty states reduce confusion during demo workflows.

## Upgrade / Run Instructions

```bash
git clone https://github.com/chumanhquan102006-creator/-cong-nghe-phan-mem.git AcademicAIAssistant
cd AcademicAIAssistant
dotnet restore AcademicAIAssistant.sln
dotnet ef database update --project AcademicAIAssistant.csproj
dotnet build AcademicAIAssistant.sln
dotnet run --project AcademicAIAssistant.csproj --launch-profile http
```

Open:

```text
http://localhost:5010
```

## Verification Commands

```bash
dotnet build
dotnet restore AcademicAIAssistant.sln -p:RestoreUseSkipNonexistentTargets=false
dotnet build AcademicAIAssistant.sln --configuration Release --no-restore
dotnet build AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --no-restore
dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --no-restore --no-build
dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj
dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --configuration Release --no-build --verbosity normal
dotnet publish AcademicAIAssistant.csproj --configuration Release --no-build --output ./publish
```

## Manual Acceptance Evidence

- Case A: uploaded, not extracted - manually verified in browser before release prep.
- Case B: extracted, no summary - manually verified in browser before release prep.
- Case C: fully processed - manually verified in browser before release prep.
- English UI - manually verified in browser before release prep.
- Vietnamese UI - manually verified in browser before release prep.
- Responsive checks - manually verified in browser before release prep.

Screenshots are not committed in this repository for this release. This document records manual verification results without fabricating visual evidence.

## Security Notes

- Cookie authentication protects private routes.
- Controllers use authenticated user identifiers to filter records.
- Password handling uses hashing logic in account flows.
- `appsettings.json` contains an empty AI API key; real keys should use User Secrets or environment variables.
- Local uploads and LocalDB are acceptable for the academic demo, not for production deployment.

## Team Contributions

See `Documentation/TEAM_CONTRIBUTIONS_v0.2.0.md` for an evidence-based contribution matrix. The strongest implementation and integration evidence belongs to Quân based on local commits, PR history, and release-preparation work. windy478 has verified issue/commit/PR evidence. Other members are documented through assigned/closed issues and verification participation unless stronger Git evidence is found.

## Known Limitations

- Academic demo, not production-ready.
- LocalDB and local uploads are development storage.
- AI retrieval and source mapping can be improved.
- OCR depends on Tesseract traineddata and image quality.
- Citation and similarity tools are rule-based aids.
- Production monitoring, scanning, storage, and secret management are out of scope.

## Future Work

- Add browser automation for the Document Workspace.
- Improve source citation mapping in document chat.
- Strengthen production secret handling and uploaded-file scanning.
- Add hosted database and managed file storage deployment options.
- Improve OCR, citation parsing, and similarity scoring.
