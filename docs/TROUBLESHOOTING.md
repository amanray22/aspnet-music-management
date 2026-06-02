# Troubleshooting

## Cannot log in with demo accounts

- Use current emails (`@mvcmusic.demo`) and password `Music@Demo2026!`.
- Old `@outlook.com` users may still exist in an old `Music.db`. Delete `Music.db` and restart, or remove users via **Maintain User Roles** as admin.
- Check `Demo:Enabled` and `Seed:DefaultPassword` / `Demo:DefaultPassword` in configuration.

## SQLite database locked

- Stop all running instances of the app.
- Delete `Music.db-shm` and `Music.db-wal` if present, then restart.

## Migrations fail

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update --project MVC_Music --startup-project MVC_Music
```

Ensure no other process holds `Music.db`.

## Empty musicians or no seed data

- Confirm `Database:SeedSampleData` is `true`.
- Check application logs for seeding errors on startup.
- Delete `Music.db` and restart for a full reseed.

## Email not sending

- Email is optional. Configure `EmailConfiguration` via User Secrets or environment variables.
- `SmtpServer` and `SmtpUsername` must both be set or `EmailSender` is not registered.

## Azure: data lost after redeploy

- Default `Music.db` in the site root may not persist. Set connection string to a path under `D:\home\site\data\`.

## Demo accounts shown twice on home page

- Ensure `DemoOptions.Accounts` is not initialized with defaults in C# **and** listed in `appsettings.json` (binding appends to lists).

## Authorization not working

- Confirm `app.UseAuthentication()` runs before `app.UseAuthorization()` in `Program.cs`.
