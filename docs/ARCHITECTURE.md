# Architecture

## Overview

MVC Music is a **classic ASP.NET Core MVC** application. Controllers use **Entity Framework Core** `DbContext` classes directly (there is no separate repository layer). Business rules live in controllers, model validation attributes, and `MusicContext` configuration.

## DbContexts

| Context | Purpose |
|---------|---------|
| `ApplicationDbContext` | ASP.NET Identity (users, roles) |
| `MusicContext` | Domain data; sets audit fields on save |

Both use the same SQLite database file (`MusicConnection`).

## Domain model (summary)

```
Genre ──< Album ──< Song ──< Performance >── Musician
                      │              │
                      └── Genre?       └── Instrument
Instrument ──< Musician (primary)
Instrument ──< Play >── Musician   (many-to-many)
Musician ──< MusicianDocument, MusicianPhoto, MusicianThumbnail
```

- **Performance** links a song, musician, and instrument (fee, comments).
- **Play** is the many-to-many join between musicians and instruments they play.
- Unique indexes: `Musician.SIN`, `(Song.Title, AlbumID)`, `(SongID, MusicianID, InstrumentID)`, `Instrument.Name`.

## Security

- Global MVC filter: authenticated users required by default.
- `HomeController` and Identity Razor Pages are anonymous.
- Role attributes on controller actions (`Staff`, `Supervisor`, `Admin`, `Security`).
- Staff/Supervisor restrictions: can only edit/delete some records they created.
- `SafeRedirectToReturnUrl` prevents open redirects from filter cookies.

## Custom controllers

| Class | Role |
|-------|------|
| `CognizantController` | Exposes controller/action names to `ViewData`; safe redirects |
| `ElephantController` | Persists list filter/sort/page URL in cookies |

## Startup seeding

1. `MusicInitializer` — migrations, SQLite triggers, instruments/genres, optional sample data.
2. `ApplicationDbInitializer` — Identity migrations, roles, demo users from `Demo:Accounts` in config.

Seeding is controlled by `Database:SeedSampleData` and `Demo:Enabled`.

## File storage

Uploaded documents and image bytes are stored in the database (`UploadedFile`, `FileContent`, `MusicianPhoto`, `MusicianThumbnail`). Images are resized to WebP with SkiaSharp.
