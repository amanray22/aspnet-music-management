# MVC Music Collection

**Live demo:** [https://amanmusiccollection.azurewebsites.net/](https://amanmusiccollection.azurewebsites.net/)

ASP.NET Core 9 MVC web application for managing musicians, instruments, albums, songs, performances, and documents. Built as a coursework/portfolio project with role-based security, optimistic concurrency, auditing, file uploads, and demo seeding for Azure.

**Author:** Aman Ray

## Table of contents

- [Features](#features)
- [Tech stack](#tech-stack)
- [Quick start](#quick-start)
- [Demo login accounts](#demo-login-accounts)
- [Configuration](#configuration)
- [Architecture](#architecture)
- [Development](#development)
- [Azure deployment](#azure-deployment)
- [Troubleshooting](#troubleshooting)
- [Project structure](#project-structure)
- [License](#license)

## Features

- CRUD for musicians, instruments, genres, albums, songs, and song performances
- ASP.NET Core Identity with roles: **Admin**, **Security**, **Supervisor**, **Staff**
- Optimistic concurrency (`RowVersion`) on musician, album, and song
- Audit fields (`CreatedBy`, `UpdatedBy`, timestamps) via `IAuditable`
- Musician photos (WebP via SkiaSharp) and document uploads with validation
- Performance summary report and Excel export (EPPlus)
- Paging, filtering, sorting; many-to-many musician ↔ instrument (`Play`)
- SQLite triggers for `RowVersion`; demo data and users seeded on startup

## Tech stack

| Technology | Use |
|------------|-----|
| .NET 9 | Runtime |
| ASP.NET Core MVC | Web UI |
| Entity Framework Core + SQLite | Data access |
| ASP.NET Core Identity | Authentication & roles |
| Bootstrap 5 | UI |
| SkiaSharp | Image resize → WebP |
| MailKit | Email (optional) |
| EPPlus | Excel import/export |

## Quick start

**Requirements:** [.NET 9 SDK](https://dotnet.microsoft.com/download)

From the repository root:

```powershell
git clone https://github.com/YOUR_USERNAME/mvc-music-collection.git
cd mvc-music-collection
dotnet restore
dotnet run --project src/MVC_Music
```

Or open `MVC_Music_Solution.sln` in Visual Studio and run **MVC_Music**.

Open the URL from the console (typically `https://localhost:7168`).

**First run:** migrations and seeding run automatically — you usually do **not** need `dotnet ef database update`.

**Reset database:** stop the app, delete `Music.db` (and `-shm`/`-wal` if present) in `src/MVC_Music/`, then run again.

## Demo login accounts

Password for all accounts: **`Music@Demo2026!`**

| Email | Role |
|-------|------|
| `admin@mvcmusic.demo` | Admin (+ Security) |
| `security@mvcmusic.demo` | Security |
| `supervisor@mvcmusic.demo` | Supervisor |
| `staff@mvcmusic.demo` | Staff |
| `guest@mvcmusic.demo` | Guest (no role) |

Accounts are defined in `src/MVC_Music/appsettings.json` under `Demo:Accounts` and shown on the home page when `Demo:ShowLoginHints` is `true`.

## Configuration

| File | Purpose |
|------|---------|
| `src/MVC_Music/appsettings.json` | Connection string, demo accounts |
| `src/MVC_Music/appsettings.Development.json` | Dev logging; optional Mailtrap host |
| `src/MVC_Music/appsettings.Production.json` | Production logging and demo seeding |

**Email (optional)** — use User Secrets, do not commit SMTP passwords:

```powershell
cd src/MVC_Music
dotnet user-secrets set "EmailConfiguration:SmtpUsername" "your-username"
dotnet user-secrets set "EmailConfiguration:SmtpPassword" "your-password"
```

**Disable demo mode (real production):**

```json
{
  "Database": { "SeedSampleData": false },
  "Demo": { "Enabled": false, "ShowLoginHints": false }
}
```

## Architecture

Controllers use **EF Core `DbContext` directly** (no repository layer). Two contexts share one SQLite database:

| Context | Purpose |
|---------|---------|
| `ApplicationDbContext` | ASP.NET Identity |
| `MusicContext` | Domain data + audit fields on save |

**Domain relationships (summary):**

```
Genre ──< Album ──< Song ──< Performance >── Musician
                      │              └── Instrument
Instrument ──< Musician (primary)
Instrument ──< Play >── Musician   (many-to-many)
```

**Security:** global `[Authorize]` on MVC controllers; `Home` and Identity pages are anonymous; role-based action attributes; Staff/Supervisor can only edit/delete some records they created; `SafeRedirectToReturnUrl` blocks open redirects.

**Custom controllers:** `CognizantController` (ViewData + safe redirects), `ElephantController` (persists filter/sort/page in cookies).

**Seeding on startup:** `MusicInitializer` (music data, triggers) and `ApplicationDbInitializer` (roles + demo users from config).

**Files:** uploads stored in the database; images resized to WebP with SkiaSharp.

## Development

**After model changes:**

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add YourMigrationName --project src/MVC_Music --startup-project src/MVC_Music
dotnet ef database update --project src/MVC_Music --startup-project src/MVC_Music
```

- Domain migrations: `src/MVC_Music/Data/MOMigrations/`
- Identity migrations: `src/MVC_Music/Data/Migrations/`

**Conventions:** filter → sort → paginate with `PaginatedList`; use `[Bind]` / `TryUpdateModelAsync` on POST; validate uploads with `FileUploadValidator`; do not pre-initialize lists in options classes that are also bound from `appsettings.json`.

## Azure deployment

1. Publish **`src/MVC_Music`** to Azure App Service (republish after moving to `src/` if needed).
2. Set `ASPNETCORE_ENVIRONMENT` = `Production`.
3. Use a **persistent** SQLite path so data survives redeploys:

   ```json
   "MusicConnection": "Data Source=D:\\home\\site\\data\\Music.db"
   ```

4. Restart once so migrations and seeding complete.

## Troubleshooting

| Issue | Fix |
|-------|-----|
| Cannot log in | Use `@mvcmusic.demo` and `Music@Demo2026!`. Delete old `Music.db` or remove stale users in **Maintain User Roles**. |
| Database locked | Stop all app instances; delete `Music.db-shm` and `Music.db-wal`; restart. |
| No seed data | Set `Database:SeedSampleData` to `true`; delete `Music.db` and restart; check startup logs. |
| Migrations fail | Install `dotnet-ef`; ensure `Music.db` is not locked; run `database update` (see Development). |
| Email not sending | Optional feature — set `EmailConfiguration` via User Secrets; both `SmtpServer` and `SmtpUsername` required. |
| Azure data lost on redeploy | Point connection string to `D:\home\site\data\Music.db`. |
| Demo accounts listed twice | Do not initialize `DemoOptions.Accounts` in C# when also defined in JSON. |

## Project structure

```
mvc-music-collection/
├── src/
│   └── MVC_Music/              # Web application (startup project)
│       ├── Controllers/
│       ├── CustomControllers/
│       ├── Data/
│       ├── Models/
│       ├── Utilities/
│       ├── ViewModels/
│       ├── Views/
│       └── wwwroot/
├── Directory.Build.props
├── README.md
├── LICENSE
└── MVC_Music_Solution.sln
```

## License

MIT — see [LICENSE](LICENSE).
