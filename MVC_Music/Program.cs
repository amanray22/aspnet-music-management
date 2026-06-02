using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MVC_Music.Data;
using MVC_Music.Utilities;
using MVC_Music.ViewModels;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MusicConnection")
    ?? throw new InvalidOperationException("Connection string 'MusicConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDbContext<MusicContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.Configure<DemoOptions>(
    builder.Configuration.GetSection(DemoOptions.SectionName));

#region For Identity System and Security

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

#endregion

var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
if (emailConfig != null
    && !string.IsNullOrWhiteSpace(emailConfig.SmtpServer)
    && !string.IsNullOrWhiteSpace(emailConfig.SmtpUsername))
{
    builder.Services.AddSingleton<IEmailConfiguration>(emailConfig);
    builder.Services.AddTransient<IEmailSender, EmailSender>();
}

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddControllersWithViews(options =>
{
    var authenticatedPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(authenticatedPolicy));
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = FileUploadValidator.MaxDocumentBytes * 5;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

var demoOptions = app.Configuration.GetSection(DemoOptions.SectionName).Get<DemoOptions>()
    ?? new DemoOptions();

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

var seedSampleData = app.Configuration.GetValue("Database:SeedSampleData", true);

if (seedSampleData)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseInitializer");

    var seedPassword = demoOptions.Enabled ? demoOptions.DefaultPassword : null;

    logger.LogInformation(
        "Seeding database (Environment: {Environment}, DemoUsers: {DemoEnabled})...",
        app.Environment.EnvironmentName,
        demoOptions.Enabled);

    MusicInitializer.Initialize(
        serviceProvider: services,
        DeleteDatabase: false,
        UseMigrations: true,
        SeedSampleData: true);

    await ApplicationDbInitializer.InitializeAsync(
        serviceProvider: services,
        useMigrations: true,
        seedSampleData: true,
        defaultPassword: seedPassword,
        demoAccounts: demoOptions.Accounts,
        logger: logger);

    logger.LogInformation("Database seeding completed.");
}

app.Run();
