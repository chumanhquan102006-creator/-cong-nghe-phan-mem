# Installation Guide

## Prerequisites

- .NET SDK
- SQL Server LocalDB
- Visual Studio or VS Code
- Git
- Browser: Chrome or Edge

## Clone Repo

```bash
git clone https://github.com/chumanhquan102006-creator/-cong-nghe-phan-mem.git AcademicAIAssistant
cd AcademicAIAssistant
```

## Restore Packages

```bash
dotnet restore AcademicAIAssistant.sln
```

## Install EF Tool If Needed

```powershell
dotnet tool install --global dotnet-ef
```

If already installed, update it if needed:

```powershell
dotnet tool update --global dotnet-ef
```

## Apply Migrations

```bash
dotnet ef database update --project AcademicAIAssistant.csproj
```

This creates or updates the SQL Server LocalDB database configured in `appsettings.json`.

## Run Project

```bash
dotnet run --project AcademicAIAssistant.csproj --launch-profile http
```

Open:

```text
http://localhost:5010
```

## Demo Seed Data

In Development, the app can create demo data automatically when it starts. The seed process is controlled by `appsettings.json`:

```json
"SeedData": {
  "Enabled": true,
  "AllowReset": true
}
```

Set `Enabled` to `false` if you want to disable demo seed data.
Set `AllowReset` to `false` if you want to disable reset from the development tools page.

Seed data is only for classroom demo/development. It does not include a real AI API key and the demo password is stored as a hash in the database.

Demo account:

- Email: `demo@student.com`
- Password: `Demo@123456`

The seeded demo workspace includes:

- Sample extracted PDF metadata and summary
- Sample essay and writing feedback
- Sample citation check
- Sample internal similarity check
- Sample OCR scan
- Sample writing coach sessions
- Sample reference manager items
- Sample chat history
- Sample knowledge graph nodes and edges

To test seed data from an empty database:

```powershell
dotnet ef database update --project AcademicAIAssistant.csproj
dotnet run --project AcademicAIAssistant.csproj --launch-profile http
```

Then login using the demo account above. Run the app a second time and confirm the demo rows are not duplicated.

## Reset Demo Data

Reset is available only in the Development environment.

1. Login to the app.
2. Open `/DevTools`.
3. Check that `SeedData.Enabled` and `SeedData.AllowReset` are both `true`.
4. Click **Reset Demo Data**.
5. Confirm the browser prompt.
6. Return to Dashboard, Documents, Writing History, Text Scan, OCR, References, and Knowledge Graph to verify the demo records were recreated.

The reset action only deletes and recreates data for the demo account:

```text
demo@student.com
```

It does not delete real user accounts or their data. Do not expose or enable this feature in Production.

## Register Account

1. Open `/Account/Register`.
2. Enter full name, email, password, and confirm password.
3. Submit the form.
4. The system redirects to Dashboard after successful registration.

You can also create your own account manually:

- Open `/Account/Register`
- Enter a new email and password
- Submit the form

## Test Main Features

1. Login.
2. Open Dashboard.
3. Upload PDF in Documents.
4. Extract text from PDF.
5. Generate summary.
6. Chat with PDF.
7. Analyze essay in Writing Studio.
8. Check citation.
9. Check similarity.
10. Build Knowledge Graph.
11. Return to Dashboard to view recent activities.

## Run Automated Tests

The xUnit test project is included in the repository and does not require a real AI API key or external API calls.

```bash
dotnet test AcademicAIAssistant.sln
```

Current verified local result:

- Build: passed
- Tests: 30/30 passed

## Continuous Integration

GitHub Actions runs restore, Release build, automated tests, publish, and artifact upload for pull requests and pushes to `main`.

Workflow file:

```text
.github/workflows/dotnet-ci.yml
```

## AI Configuration

AI is optional. The app works in rule-based fallback mode when AI is disabled.

Default `appsettings.json`:

```json
"AI": {
  "Provider": "OpenAI",
  "ApiKey": "",
  "Endpoint": "https://api.openai.com/v1/chat/completions",
  "Model": "gpt-4o-mini",
  "Enabled": false,
  "TimeoutSeconds": 30
}
```

Do not commit real API keys.

Use User Secrets for local AI testing:

```powershell
dotnet user-secrets set "AI:ApiKey" "YOUR_API_KEY"
dotnet user-secrets set "AI:Enabled" "true"
```

## Common Issues

### `dotnet ef` not found

Run:

```powershell
dotnet tool install --global dotnet-ef
```

### LocalDB connection error

Check that SQL Server LocalDB is installed, then run:

```powershell
sqllocaldb info
```

### Port already in use

Stop the existing running app or change `applicationUrl` in `Properties/launchSettings.json`.

### PDF text is empty

The PDF may be scanned/image-only. Use a text-based PDF for the demo.
