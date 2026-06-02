using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVC_Music.ViewModels;

namespace MVC_Music.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider,
            bool useMigrations = true,
            bool seedSampleData = true,
            string? defaultPassword = null,
            IReadOnlyList<DemoAccount>? demoAccounts = null,
            ILogger? logger = null)
        {
            if (useMigrations)
            {
                await using var context = new ApplicationDbContext(
                    serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());
                try
                {
                    await context.Database.MigrateAsync();
                    logger?.LogInformation("Identity database migrations applied.");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to apply Identity database migrations.");
                }
            }

            if (!seedSampleData)
            {
                logger?.LogInformation("Sample data seeding is disabled.");
                return;
            }

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            try
            {
                string[] roleNames = ["Admin", "Security", "Supervisor", "Staff"];
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger?.LogInformation("Created role {RoleName}.", roleName);
                    }
                }

                if (string.IsNullOrWhiteSpace(defaultPassword))
                {
                    logger?.LogWarning("Demo user seeding skipped: no DefaultPassword configured.");
                    return;
                }

                if (demoAccounts == null || demoAccounts.Count == 0)
                {
                    logger?.LogWarning("Demo user seeding skipped: no demo accounts configured.");
                    return;
                }

                foreach (var account in demoAccounts)
                {
                    if (string.IsNullOrWhiteSpace(account.Email))
                    {
                        continue;
                    }

                    await EnsureUserAsync(
                        userManager,
                        account.Email,
                        defaultPassword,
                        logger,
                        account.IdentityRoles.ToArray());
                }

                logger?.LogInformation("Demo identity accounts are ready.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed while seeding Identity data.");
            }
        }

        private static async Task EnsureUserAsync(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            ILogger? logger,
            params string[] roles)
        {
            if (await userManager.FindByEmailAsync(email) != null)
            {
                return;
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger?.LogWarning("Could not create demo user {Email}: {Errors}",
                    email, string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            foreach (var role in roles)
            {
                await userManager.AddToRoleAsync(user, role);
            }

            logger?.LogInformation("Created demo user {Email} with roles [{Roles}].",
                email, string.Join(", ", roles));
        }
    }
}
