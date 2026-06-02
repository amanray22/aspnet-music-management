namespace MVC_Music.ViewModels
{
    public class DemoOptions
    {
        public const string SectionName = "Demo";

        public bool Enabled { get; set; } = true;

        public bool ShowLoginHints { get; set; } = true;

        public string DefaultPassword { get; set; } = "Music@Demo2026!";

        // Do not initialize with defaults here — configuration binding appends to
        // existing list items, which would duplicate entries from appsettings.json.
        public List<DemoAccount> Accounts { get; set; } = [];
    }

    public class DemoAccount
    {
        public string Email { get; set; } = "";

        /// <summary>Display label on the home page demo table.</summary>
        public string Role { get; set; } = "";

        /// <summary>ASP.NET Identity roles assigned at seed time.</summary>
        public List<string> IdentityRoles { get; set; } = [];
    }
}
