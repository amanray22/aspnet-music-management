# MVC Music Collection

**Live demo:** [https://amanmusiccollection.azurewebsites.net/](https://amanmusiccollection.azurewebsites.net/)

ASP.NET Core 9 MVC web application for managing musicians, instruments, albums, songs, performances, and documents. Built as a coursework/portfolio project with enterprise-style patterns: role-based security, optimistic concurrency, auditing, file uploads, and demo seeding for Azure presentations.

**Author:** Aman Ray

## Table of contents

- [Features](#features)
- [Tech stack](#tech-stack)
- [Quick start](#quick-start)
- [Demo login accounts](#demo-login-accounts)
- [Configuration](#configuration)
- [Azure deployment](#azure-deployment)
- [Project structure](#project-structure)
- [Documentation](#documentation)
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

```powershell
git clone https://github.com/YOUR_USERNAME/MVC_Music_Solution.git
cd MVC_Music_Solution/MVC_Music
dotnet restore
dotnet run
```

Open the URL from the console (typically `https://localhost:7168`).

**First run:** migrations apply automatically; sample music data and demo users are seeded (no manual `dotnet ef database update` required).

To reset the database: stop the app, delete `Music.db` (and `-shm`/`-wal` if present) in the `MVC_Music` folder, then run again.

## Demo login accounts

Password for all accounts: **`Music@Demo2026!`**

| Email | Role |
|-------|------|
| `admin@mvcmusic.demo` | Admin (+ Security) |
| `security@mvcmusic.demo` | Security |
| `supervisor@mvcmusic.demo` | Supervisor |
| `staff@mvcmusic.demo` | Staff |
| `guest@mvcmusic.demo` | Guest (no role) |

The home page lists these when `Demo:ShowLoginHints` is `true`. Accounts are defined in `appsettings.json` under `Demo:Accounts`.

## Configuration

| File | Purpose |
|------|---------|
| `appsettings.json` | Connection string, demo accounts, base settings |
| `appsettings.Development.json` | Dev logging; optional Mailtrap host (credentials via User Secrets) |
| `appsettings.Production.json` | Production logging; demo seeding enabled for hosted demo |

**Email (optional):**

```powershell
cd MVC_Music
dotnet user-secrets set "EmailConfiguration:SmtpUsername" "your-username"
dotnet user-secrets set "EmailConfiguration:SmtpPassword" "your-password"
```

**Azure SQLite path (recommended):**

```json
"MusicConnection": "Data Source=D:\\home\\site\\data\\Music.db"
```

## Azure deployment

1. Publish the `MVC_Music` project to Azure App Service.
2. Set `ASPNETCORE_ENVIRONMENT` = `Production`.
3. Use a **persistent** SQLite path (see above).
4. Restart once so migrations and seeding complete.

## Project structure

```
MVC_Music_Solution/
├── MVC_Music/                 # Web application (startup project)
│   ├── Controllers/
│   ├── CustomControllers/     # CognizantController, ElephantController
│   ├── Data/                  # DbContexts, migrations, seeding
│   ├── Models/
│   ├── Utilities/
│   ├── ViewModels/
│   ├── Views/
│   └── wwwroot/
├── docs/                      # Extended documentation
├── README.md
├── LICENSE
└── MVC_Music_Solution.sln
```

## Documentation

| Document | Description |
|----------|-------------|
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Data model, security, and request flow |
| [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) | Local setup, migrations, conventions |
| [docs/CONTRIBUTING.md](docs/CONTRIBUTING.md) | How to contribute |
| [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) | Common issues |

## Disable demo mode (real production)

```json
{
  "Database": { "SeedSampleData": false },
  "Demo": { "Enabled": false, "ShowLoginHints": false }
}
```

## License

MIT — see [LICENSE](LICENSE).
