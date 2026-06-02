using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MVC_Music.Models;
using MVC_Music.ViewModels;
using System.Diagnostics;

namespace MVC_Music.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DemoOptions _demoOptions;
        private readonly IWebHostEnvironment _environment;

        public HomeController(
            ILogger<HomeController> logger,
            IOptions<DemoOptions> demoOptions,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _demoOptions = demoOptions.Value;
            _environment = environment;
        }

        public IActionResult Index()
        {
            ViewData["ShowDemoAccounts"] = _demoOptions.ShowLoginHints && _demoOptions.Enabled;
            ViewData["DemoPassword"] = _demoOptions.DefaultPassword;
            ViewData["DemoAccounts"] = _demoOptions.Accounts;
            ViewData["IsProduction"] = !_environment.IsDevelopment();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
