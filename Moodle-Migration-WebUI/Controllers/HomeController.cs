using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moodle_Migration_WebUI.Models;
using System.Diagnostics;
using System.Security.Claims;
using Moodle_Migration_WebUI.Data;
using Microsoft.Extensions.Options;

namespace Moodle_Migration_WebUI.Controllers
{
    public class HomeController : Controller
    {
        private const string Username = "admin";
        private const string Password = "adminP@ssw0rd!";

        private readonly ILogger<HomeController> _logger;
        private readonly LoginCredentials _loginCredentials;

        public HomeController(ILogger<HomeController> logger, IOptions<LoginCredentials> loginCredentials)
        {
            _logger = logger;
            _loginCredentials = loginCredentials.Value;
        }

        public IActionResult Index()
        {
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
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous, HttpPost]
        public IActionResult Login(AuthUser authUser)
        {
            if (ModelState.IsValid)
            {
                if (authUser.Username == _loginCredentials.Username && authUser.Password == _loginCredentials.Password)
                {
                    var claims = new[] { new Claim("name", authUser.Username), new Claim(ClaimTypes.Role, "Admin") };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
                            new AuthenticationProperties
                            {
                                IsPersistent = true, // Optional for persistent cookies
                                ExpiresUtc = DateTime.UtcNow.AddMinutes(60), // Matches your configuration
                                AllowRefresh = true
                            });

                    string learnerName = authUser.Username + " " + authUser.Password;

                    HttpContext.Session.SetString("IsLoggedIn", "true"); // Set session
                    return RedirectToAction("Index", "Command");

                }
                else
                {
                    ModelState.AddModelError("", "Login failed. Please verify Username");
                }
            }

            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.SetString("IsLoggedIn", "false"); // Set session
            if (!(this.User?.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Home");
        }
    }
}
