using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;
using System.Security.Claims;

namespace GamificationPlatform.Controllers
{
    public class UserController : Controller
    {
        private readonly ChallengeDbContext _challengeDbContext;

        public UserController(ChallengeDbContext challengeDbContext)
        {
            _challengeDbContext = challengeDbContext;
        }

        // Shows all users
        public async Task<IActionResult> Table()
        {
            List<User> users =
                await _challengeDbContext.Users.ToListAsync();

            return View(users);
        }

        // Shows the registration form
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Creates a new user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email
                };

                var passwordHasher = new PasswordHasher<User>();

                user.PasswordHash =
                    passwordHasher.HashPassword(user, model.Password);

                _challengeDbContext.Users.Add(user);

                await _challengeDbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }

        // Shows the login form
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Logs the user in
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _challengeDbContext.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username or password.");

                return View(model);
            }

            var passwordHasher =
                new PasswordHasher<User>();

            var result =
                passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username or password.");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Username)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var claimsPrincipal =
                new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal);

            return RedirectToAction(
                "Grid",
                "Challenge");
        }

        // Logs the user out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                "Index",
                "Home");
        }

    }
}