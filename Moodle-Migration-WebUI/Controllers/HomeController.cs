using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moodle_Migration_WebUI.Models;
using System.Diagnostics;
using System.Security.Claims;
using Moodle_Migration_WebUI.Data;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Moodle_Migration_WebUI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Moodle_Migration_WebUI.Controllers
{
    public class HomeController : Controller
    {
        private const string Username = "admin";
        private const string Password = "adminP@ssw0rd!";

        private readonly ILogger<HomeController> _logger;
        private readonly LoginCredentials _loginCredentials;
        private readonly LoggingDBContext _dbContext;

        public HomeController(ILogger<HomeController> logger, IOptions<LoginCredentials> loginCredentials, LoggingDBContext dbContext)
        {
            _logger = logger;
            _loginCredentials = loginCredentials.Value;
            _dbContext = dbContext;
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
        public async Task<IActionResult> Login(AuthUser authUser)
        {
            if (ModelState.IsValid)
            {
                var user = await _dbContext.UserTBL.FirstOrDefaultAsync(u => u.UserName == authUser.Username);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(); // Or redirect with error
                }

                var hasher = new PasswordHasher<User>();
                //string hash = hasher.HashPassword(user, password);
                var result = hasher.VerifyHashedPassword(user, user.PasswordHash, authUser.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, authUser.Username), new Claim(ClaimTypes.NameIdentifier, authUser.Username), new Claim(ClaimTypes.Role, "Admin") };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                 await   HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
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

                ModelState.AddModelError("", "Invalid username or password.");
                return View(); // Or redirect with error
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
