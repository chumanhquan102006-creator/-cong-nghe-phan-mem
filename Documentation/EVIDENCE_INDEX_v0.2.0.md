# v0.2.0 Rubric Evidence Index

## 1. Product Vision and User Scenarios

Theory: user-centered development and scenario-based requirements.

Engineering decision: reposition the product from a feature toolbox to a document-centered academic workflow.

Files: `README.md`, `Documentation/PRODUCT_ENGINEERING.md`, `Documentation/RELEASE_NOTES_v0.2.0.md`, `SELF_ASSESSMENT.md`.

Commits / PRs: `e988b14`, `d72137b`, `1395732`, `a00f7d3`, `fc8e8e0`, PR #20, PR #27, PR #39.

Result: clearer upload-to-summary-to-question flow in the Document Workspace.

Limitation: advanced browser evidence is still limited.

## 2. Functionality and Product Value

Theory: useful software should support real user goals, not only isolated features.

Engineering decision: preserve the broad academic toolbox while improving the main reading workflow.

Files: `README.md`, `Documentation/PRODUCT_ENGINEERING.md`, `Documentation/RELEASE_NOTES_v0.2.0.md`, `Documentation/TEST_REPORT_v0.2.0.md`.

Commits / PRs: `1395732`, `a00f7d3`, `fc8e8e0`, PR #27, PR #39.

Result: broad feature coverage with improved document value in v0.2.0.

Limitation: AI retrieval and source mapping remain basic.

## 3. Architecture and Design Rationale

Theory: separation of concerns, layered architecture, and presentation models.

Engineering decision: MVC controllers/views, service layer, EF Core data layer, optional AI integration, and `DocumentWorkspaceViewModel`.

Files: `Documentation/ARCHITECTURE.md`, `Program.cs`, `Controllers/`, `Services/`, `Models/ViewModels/`, `Data/`.

Commits / PRs: `1395732`, `a00f7d3`, `fc8e8e0`, PR #27.

Result: understandable architecture with release-specific rationale.

Limitation: production deployment architecture is not implemented.

## 4. Code Quality and Reliable Programming

Theory: defensive programming, graceful degradation, maintainability.

Engineering decision: rule-based fallback, TempData string serialization, short-question validation, state-aware document and OCR actions.

Files: `Services/`, `Controllers/`, `Views/Documents/Details.cshtml`, `Views/DocumentChat/_ChatPanel.cshtml`, `Documentation/RELEASE_NOTES_v0.2.0.md`.

Commits / PRs: `e988b14`, `d72137b`, `a00f7d3`, `fc8e8e0`, PR #20, PR #27.

Result: fewer demo-breaking states and clearer user feedback.

Limitation: some end-to-end behavior is still manually tested.

## 5. Testing and Verification

Theory: verification should combine automated repeatability with manual acceptance for UI-heavy flows.

Engineering decision: xUnit service/view-model tests plus documented manual browser checks.

Files: `AcademicAIAssistant.Tests/`, `Documentation/TEST_REPORT_v0.2.0.md`, `.github/workflows/dotnet-ci.yml`.

Commits / PRs: `fc8e8e0`, `6e23bff`, PR #39.

Result: automated tests and manual matrix cover Document Workspace states and localization/responsive checks.

Limitation: no committed screenshots and no automated browser test suite.

## 6. Security and Privacy

Theory: least privilege, authentication, authorization, and secret hygiene.

Engineering decision: cookie auth, `[Authorize]`, user-scoped queries, empty committed API key, ignored uploads/databases/secrets.

Files: `Program.cs`, controllers, `.gitignore`, `appsettings.json`, `Documentation/SECURITY_AND_PRIVACY.md`.

Commits / PRs: `e988b14`, `d72137b`, `fc8e8e0`, PR #20, PR #27.

Result: appropriate academic-demo safeguards are documented.

Limitation: LocalDB, local uploads, no production monitoring, limited file scanning.

## 7. DevOps and Reproducibility

Theory: repeatable restore/build/test/publish and release traceability.

Engineering decision: GitHub Actions CI, release branch, PR, squash merge plan, tag/release asset plan.

Files: `.github/workflows/dotnet-ci.yml`, `CHANGELOG.md`, `Documentation/RELEASE_CHECKLIST_v0.2.0.md`, `Documentation/RELEASE_NOTES_v0.2.0.md`.

Commits / PRs: `6e23bff`, PR #39.

Result: CI pipeline remains restore/build/test/publish/artifact capable.

Limitation: remote CI, tag, and release verification still need completion.

## 8. Documentation and Theory-to-Practice Traceability

Theory: engineering decisions should be traceable to principles and evidence.

Engineering decision: add release notes, test report, architecture, security/privacy, contribution matrix, evidence index, and this self-assessment.

Files: `Documentation/`, `SELF_ASSESSMENT.md`.

Commits / PRs: `6e23bff`, PR #39.

Result: rubric evidence is easier to verify without fabricating screenshots, ownership, or production claims.

Limitation: individual non-code contributions would be stronger with linked issue comments or PR reviews.
