# Development guide

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022, VS Code, or Rider (optional)
- `dotnet-ef` global tool (only if you change models manually)

```powershell
dotnet tool install --global dotnet-ef
```

## Run locally

```powershell
cd MVC_Music
dotnet restore
dotnet build
dotnet run
```

Launch profiles in `Properties/launchSettings.json`:

| Profile | Environment | URL |
|---------|-------------|-----|
| https | Development | `https://localhost:7168` |
| Production | Production | Same ports; tests prod config |

## Database and migrations

- **Provider:** SQLite (`Data Source=Music.db` in project folder).
- **Auto-migrate:** `Program.cs` calls initializers on startup — you usually do **not** need `dotnet ef database update`.
- **Manual update** (after model changes):

```powershell
dotnet ef migrations add YourMigrationName --project MVC_Music --startup-project MVC_Music
dotnet ef database update --project MVC_Music --startup-project MVC_Music
```

Domain migrations: `Data/MOMigrations/`  
Identity migrations: `Data/Migrations/`

## Coding conventions (this project)

- Follow existing controller patterns: filter → sort → paginate with `PaginatedList`.
- Use `[Authorize]` and role attributes; do not rely on hiding links alone.
- Use `[Bind(...)]` or `TryUpdateModelAsync` with explicit property lists on POST.
- Use `SafeRedirectToReturnUrl` instead of raw `Redirect(url)`.
- Validate uploads with `FileUploadValidator`.
- Do not initialize lists in options classes if the same data comes from `appsettings.json` (binding appends to lists).

## Configuration secrets

Do not commit SMTP passwords. Use User Secrets (ID in `.csproj`):

```powershell
dotnet user-secrets set "EmailConfiguration:SmtpUsername" "value"
dotnet user-secrets set "EmailConfiguration:SmtpPassword" "value"
```

## What this project does *not* use

- Repository pattern (data access is via `MusicContext` in controllers)
- SQL Server (SQLite only in current codebase)
- `src/` folder layout (project is `MVC_Music/` at solution root)
