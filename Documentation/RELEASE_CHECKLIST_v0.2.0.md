# Release Checklist v0.2.0

- [x] Working tree clean at baseline
- [ ] Working tree clean after release-prep commit
- [x] Build pass
- [x] 46/46 tests pass
- [x] No secrets found in audited config/docs
- [x] No database schema change
- [x] No migration
- [x] README updated
- [x] CHANGELOG updated
- [x] Release notes created
- [x] Test report created
- [x] Architecture evidence created
- [x] Security evidence created
- [x] Team contribution evidence created
- [x] Self-assessment created
- [ ] CI pass
- [ ] PR merged
- [ ] Tag verified
- [ ] GitHub Release published
- [ ] Release asset verified

## Notes

- Baseline branch: `feature/local-ui-ux-improvements`.
- Release preparation branch: `release/v0.2.0`.
- Baseline commit: `fc8e8e0 fix(documents): finish document workspace polish`.
- Local automated test result: 46 passed, 0 failed, 0 skipped.
- Bare `dotnet build` reported failure with 0 warnings and 0 errors.
- `dotnet restore AcademicAIAssistant.sln -p:RestoreUseSkipNonexistentTargets=false` passed locally.
- Release build passed with 1 MSBuild cache warning and 0 errors after sandbox-permission rerun.
- Release publish to ignored `publish/` folder passed.
- No screenshots are committed for manual browser checks.
