# Contributing

Thanks for your interest in MVC Music Collection.

## Getting started

1. Fork the repository.
2. Clone your fork and create a branch:

   ```bash
   git checkout -b feature/your-feature-name
   ```

3. Make changes and test locally:

   ```powershell
   cd MVC_Music
   dotnet build
   dotnet run
   ```

4. Commit with a clear message:

   ```bash
   git commit -m "feat: describe your change"
   ```

5. Push and open a Pull Request.

## Guidelines

- Match existing MVC and naming conventions.
- Keep changes focused; avoid unrelated refactors.
- Add or update EF migrations if you change models.
- Do not commit `bin/`, `obj/`, `.vs/`, or `*.db` files.
- Do not commit production SMTP secrets.
- Demo passwords in `appsettings.json` are intentional for the sample app; document any changes in the PR.

## Pull request checklist

- [ ] Builds with `dotnet build`
- [ ] Tested login and at least one CRUD flow
- [ ] Migrations included if models changed
- [ ] README or `docs/` updated if behavior or setup changed
