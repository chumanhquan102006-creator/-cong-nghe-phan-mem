# Test Report v0.2.0

## Automated Verification

Environment:

- OS: Windows 10
- .NET SDK: 9.0.315
- Runtime target: `net9.0`
- Branch: `release/v0.2.0`
- Baseline commit: `fc8e8e0 fix(documents): finish document workspace polish`

Commands executed during release preparation:

```bash
git branch --show-current
git status
git log --oneline -15
git remote -v
dotnet build
dotnet restore AcademicAIAssistant.sln -p:RestoreUseSkipNonexistentTargets=false
dotnet build AcademicAIAssistant.sln --configuration Release --no-restore
dotnet build AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --no-restore
dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --no-restore --no-build
dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj
dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --configuration Release --no-build --verbosity normal
dotnet publish AcademicAIAssistant.csproj --configuration Release --no-build --output ./publish
```

Results:

| Command | Result |
| --- | --- |
| `git branch --show-current` | `feature/local-ui-ux-improvements` before release branch creation |
| `git status` | Working tree clean |
| `git log --oneline -15` | Included `fc8e8e0`, `a00f7d3`, `1395732`, `d72137b`, `e988b14` |
| `dotnet build` | Failed during restore/solution build selection with 0 warnings and 0 errors reported |
| `dotnet restore AcademicAIAssistant.sln -p:RestoreUseSkipNonexistentTargets=false` | Passed; all projects up-to-date for restore |
| `dotnet build AcademicAIAssistant.sln --configuration Release --no-restore` | Passed with 1 MSBuild cache warning and 0 errors after rerun outside sandbox |
| `dotnet build AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --no-restore` | Passed, 0 warnings, 0 errors |
| `dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --no-restore --no-build` | Passed: 46, Failed: 0, Skipped: 0, Total: 46 |
| `dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj` | Passed: 46, Failed: 0, Skipped: 0, Total: 46 |
| `dotnet test AcademicAIAssistant.Tests\AcademicAIAssistant.Tests.csproj --configuration Release --no-build --verbosity normal` | Passed: 46, Failed: 0, Skipped: 0, Total: 46 |
| `dotnet publish AcademicAIAssistant.csproj --configuration Release --no-build --output ./publish` | Passed; output written to ignored `publish/` folder |

The bare `dotnet build` and plain solution restore behavior is recorded honestly. Project-level restore succeeds, and solution restore succeeds locally when `RestoreUseSkipNonexistentTargets=false` is supplied. The test project build, Debug tests, Release tests, and publish command passed.

## Test Growth

Repository history supports this growth path:

- `v0.1.0`: 30 tests, recorded in `README.md`, `CHANGELOG.md`, and `Documentation/RELEASE_NOTES_v0.1.0.md`.
- P0 cleanup: 34 tests, represented by local test expansion around `d72137b fix(ux): clean up critical product friction`.
- Document Workspace: 44 tests, represented by the workspace recovery checkpoint `a00f7d3`.
- Workspace polish: 46 tests, verified at `fc8e8e0` and on `release/v0.2.0`.

## Automated Coverage Areas

- File validation for PDF/JPG/PNG upload rules.
- Rule-based document summaries.
- Rule-based writing feedback.
- APA citation checking.
- Reference generation.
- Login rate limiting.
- Internal text similarity with EF Core InMemory.
- Document chat short-question and grounded-response behavior.
- PDF text normalization.
- Document Workspace view-model tab and state behavior.

## Manual Acceptance Matrix

| ID | Scenario | Steps | Expected | Actual | Result |
| --- | --- | --- | --- | --- | --- |
| A-01 | Upload only | Upload a PDF and open Document Workspace before extraction | Workspace shows upload state and blocks summary/chat actions that need text | Browser verification completed manually before release prep | Pass |
| B-01 | Text extracted, no summary | Extract text, do not generate summary, open workspace | Summary can be generated; Ask AI is available from extracted text | Browser verification completed manually before release prep | Pass |
| C-01 | Fully ready document | Extract text and generate summary | Overview, Summary, and Ask AI all show ready state | Browser verification completed manually before release prep | Pass |
| CHAT-01 | Short question | Ask a short valid question in Document Chat | Question is accepted instead of rejected by length-only rules | Covered by automated tests and browser smoke verification | Pass |
| CHAT-02 | Grounded answer | Ask about extracted document content | Response uses document text or fallback context | Covered by automated tests and browser smoke verification | Pass |
| OCR-01 | Empty OCR result | Upload an image with no readable OCR text | App shows friendly empty/failure message and avoids invalid follow-up actions | Browser verification completed manually before release prep | Pass |
| I18N-01 | English | Use main document/OCR flows in English | Labels and messages are readable and consistent | Browser verification completed manually before release prep | Pass |
| I18N-02 | Vietnamese | Switch to Vietnamese and use main flows | Labels and messages are readable and consistent | Browser verification completed manually before release prep | Pass |
| RESP-01 | Desktop | Check main workspace on desktop viewport | Layout is usable without overlapping content | Browser verification completed manually before release prep | Pass |
| RESP-02 | Mobile | Check main workspace on mobile viewport | Layout remains usable and actions stay reachable | Browser verification completed manually before release prep | Pass |

No screenshots are committed for this release. The manual checks above are recorded as manual browser verification, not as fabricated artifacts.
