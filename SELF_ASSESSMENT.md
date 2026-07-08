# Group Self-Assessment - /80

This is an evidence-based self-assessment proposal. Final grading belongs to the instructor.

## 1. Product Vision and User Scenarios

Theory -> user-centered development and scenario-based requirements.

Engineering decision -> reposition the product from a feature toolbox to a document-centered academic workflow.

Repository evidence -> `README.md`, `Documentation/PRODUCT_ENGINEERING.md`, `Documentation/RELEASE_NOTES_v0.2.0.md`, Document Workspace views and view model.

Result -> students can follow a clearer flow from upload to extraction to summary to questions.

Limitation -> some advanced scenarios still need stronger automated browser evidence.

Proposed score: 9/10.

## 2. Functionality and Product Value

Theory -> useful software should support real user goals, not only isolated features.

Engineering decision -> preserve the broad academic toolbox while improving the most important reading workflow.

Repository evidence -> documents, summaries, chat, OCR, writing, citation, similarity, references, graph, settings, and dashboard modules.

Result -> broad feature set with improved document value in `v0.2.0`.

Limitation -> AI retrieval/source mapping remains basic.

Proposed score: 13/14.

## 3. Architecture and Design Rationale

Theory -> separation of concerns, layered architecture, and presentation models.

Engineering decision -> MVC controllers/views, service layer, EF Core data layer, optional AI integration, and `DocumentWorkspaceViewModel`.

Repository evidence -> `Documentation/ARCHITECTURE.md`, `Program.cs`, `Controllers/`, `Services/`, `Models/ViewModels/`, `Data/`.

Result -> understandable architecture with release-specific rationale.

Limitation -> production deployment architecture is not implemented.

Proposed score: 10/12.

## 4. Code Quality and Reliable Programming

Theory -> defensive programming, graceful degradation, maintainability.

Engineering decision -> rule-based fallback, TempData string serialization, short-question validation, state-aware document and OCR actions.

Repository evidence -> `Services/`, `Controllers/`, `Views/Documents/Details.cshtml`, `Views/DocumentChat/_ChatPanel.cshtml`, v0.2.0 commits.

Result -> fewer demo-breaking states and clearer user feedback.

Limitation -> some end-to-end behavior is still manually tested.

Proposed score: 9/10.

## 5. Testing and Verification

Theory -> verification should combine automated repeatability with manual acceptance for UI-heavy flows.

Engineering decision -> xUnit service/view-model tests plus documented manual browser checks.

Repository evidence -> `AcademicAIAssistant.Tests/`, `Documentation/TEST_REPORT_v0.2.0.md`, GitHub Actions workflow.

Result -> 46 automated tests verified locally; manual matrix records Document Workspace states and localization/responsive checks.

Limitation -> no committed screenshots and no automated browser test suite.

Proposed score: 8/9.

## 6. Security and Privacy

Theory -> least privilege, authentication, authorization, and secret hygiene.

Engineering decision -> cookie auth, `[Authorize]`, user-scoped queries, empty committed API key, ignored uploads/databases/secrets.

Repository evidence -> `Program.cs`, controllers, `.gitignore`, `appsettings.json`, `Documentation/SECURITY_AND_PRIVACY.md`.

Result -> appropriate academic-demo safeguards are documented.

Limitation -> LocalDB, local uploads, no production monitoring, limited file scanning.

Proposed score: 7/8.

## 7. DevOps and Reproducibility

Theory -> repeatable restore/build/test/publish and release traceability.

Engineering decision -> GitHub Actions CI, release branch, PR, squash merge plan, tag/release asset plan.

Repository evidence -> `.github/workflows/dotnet-ci.yml`, changelog, release notes, release checklist.

Result -> CI pipeline remains restore/build/test/publish/artifact capable.

Limitation -> local bare `dotnet build` failed silently during baseline; remote CI must pass before merge/release.

Proposed score: 8/9.

If the v0.2.0 PR CI, publish artifact, squash merge, tag, and GitHub Release are all verified successfully, this DevOps proposal may be reassessed from 8/9 to 9/9, giving 73/80.

## 8. Documentation and Theory-to-Practice Traceability

Theory -> engineering decisions should be traceable to principles and evidence.

Engineering decision -> add release notes, test report, architecture, security/privacy, contribution matrix, and this self-assessment.

Repository evidence -> `Documentation/` and `SELF_ASSESSMENT.md`.

Result -> strong rubric evidence without fabricating screenshots, ownership, or production claims.

Limitation -> individual non-code contributions would be stronger with linked issue comments or PR reviews.

Proposed score: 8/8.

## Proposed Group Total

Proposed evidence-based group score: 72/80.

# Individual Self-Assessment - /20 Each

## Quan

### Meaningful Technical Contribution - 7/7

Evidence: primary local v0.2.0 implementation commits `e988b14`, `d72137b`, `1395732`, `a00f7d3`, `fc8e8e0`; authentication/admin/dashboard/UI/UX/release work; PR #20, PR #27, PR #39; integration across controllers, services, views, localization, tests, and CI.

### Theory-Informed Ownership - 5/5

Evidence: product workflow redesign, incremental delivery, MVC/service architecture, reliable programming fixes, testing strategy, and documented trade-offs in `Documentation/ARCHITECTURE.md` and `Documentation/PRODUCT_ENGINEERING.md`.

### Collaboration & Agile Leadership - 3/3

Evidence: issue organization, branches, PR integration, release evidence rebuild, and v0.2.0 release leadership.

### Testing, Documentation, DevOps & Quality - 3/3

Evidence: test growth to 46, CI workflow, release notes, test report, architecture/security docs, contribution matrix, release checklist, and release-prep verification.

### Reflection & Improvement - 2/2

Reflection: the project initially prioritized feature count and became a toolbox. Browser review showed workflow and usability problems. The direction changed toward a workflow-oriented design centered on the Document Workspace. Current limitation: AI retrieval and source mapping can still be improved.

Proposed total: 20/20.

## Windy

Evidence: issue #18, #25, #36; commits `45de67a`, `8a632db`, `ff9b114`; PR #19, PR #26, and PR #40 merge evidence, including merge commit `87ab919`.

Suggested evidence-based range: 11-12/20.

## Phuong Anh

Evidence: assigned/closed issues #4, #8, #10, #13, #17, #23; task participation, functional verification, acceptance checking, workflow review, and integration feedback. No local commits/PRs were found for this account in the audit.

Suggested evidence-based range: 8-10/20.

## Ngoc Quy

Evidence: assigned/closed issues #1, #5, #15, #22, #30, #32; task participation, functional verification, acceptance review, and OCR/document/error-page workflow checks. No local commits/PRs were found for this account in the audit.

Suggested evidence-based range: 8-10/20.

## Quang Thang

Evidence: assigned/closed issues #2, #7, #12, #16, #24; task participation, functional verification, acceptance review, and similarity/export/date/UI quality checks. No local commits/PRs were found for this account in the audit.

Suggested evidence-based range: 8-10/20.
