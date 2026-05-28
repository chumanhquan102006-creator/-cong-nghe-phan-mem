# Installation Guide

## Prerequisites

- .NET SDK
- SQL Server LocalDB
- Visual Studio or VS Code
- Git
- Browser: Chrome or Edge

## Clone Repo

```powershell
git clone <repository-url>
cd AcademicAIAssistant
```

If the project is already on your computer, open the project folder directly:

```powershell
cd "D:\viết luận\AcademicAIAssistant"
```

## Restore Packages

```powershell
dotnet restore
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

```powershell
dotnet ef database update
```

This creates or updates the SQL Server LocalDB database configured in `appsettings.json`.

## Run Project

```powershell
dotnet run --launch-profile http
```

Open:

```text
http://localhost:5010
```

## Register Account

1. Open `/Account/Register`.
2. Enter full name, email, password, and confirm password.
3. Submit the form.
4. The system redirects to Dashboard after successful registration.

Suggested demo account:

- Email: `demo@student.local`
- Password: `Demo@123`

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
