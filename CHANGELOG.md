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

### Known Limitations

- Similarity scan is internal only, not Internet-based and not Turnitin.
- Citation checking is basic and does not guarantee full academic correctness.
- AI output should be reviewed by students before use.
- Gemini API requires user configuration; fallback logic is available when AI is disabled or unavailable.
- Some edge cases may not be fully supported.
