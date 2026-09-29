using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
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

        public UserController(
            ChallengeDbContext challengeDbContext)
        {
            _challengeDbContext = challengeDbContext;
        }


        // Shows all users - admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Table()
        {
            List<User> users =
                await _challengeDbContext.Users
                    .Include(u => u.CreatedChallenges)
                    .ToListAsync();

            return View(users);
        }


        // Shows details and history for one user - admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var user =
                await _challengeDbContext.Users
                    .Include(u => u.CreatedChallenges)
                        .ThenInclude(c => c.Questions)
                    .Include(u => u.UserChallenges)
                        .ThenInclude(uc => uc.Challenge)
                    .Include(u => u.UserChallenges)
                        .ThenInclude(uc => uc.Attempts)
                    .FirstOrDefaultAsync(u =>
                        u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Calculates max points for challenges
            // created by this user.
            foreach (var challenge in user.CreatedChallenges)
            {
                challenge.MaxPoints =
                    challenge.Questions.Sum(q => q.Points);
            }

            var viewModel =
                new UserDetailsViewModel
                {
                    User = user,

                    CreatedChallenges =
                        user.CreatedChallenges,

                    // Only includes challenges
                    // that the user has completed.
                    ChallengeHistory =
                        user.UserChallenges
                            .Where(uc =>
                                uc.Attempts.Any(a =>
                                    a.Completed))
                            .ToList()
                };

            return View(viewModel);
        }


        // Shows the registration form
        [HttpGet]
        public IActionResult Register(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // Creates a new user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,

                    // New users are normal users by default
                    IsAdmin = false
                };

                var passwordHasher =
                    new PasswordHasher<User>();

                user.PasswordHash =
                    passwordHasher.HashPassword(
                        user,
                        model.Password);

                _challengeDbContext.Users.Add(user);

                await _challengeDbContext
                    .SaveChangesAsync();

                // Sends the ReturnUrl to the login page
                return RedirectToAction(
                    nameof(Login),
                    new
                    {
                        returnUrl = returnUrl
                    });
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(model);
        }


        // Shows the login form
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            var model =
                new LoginViewModel
                {
                    ReturnUrl = returnUrl
                };

            return View(model);
        }


        // Logs the user in
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user =
                await _challengeDbContext.Users
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

            if (result ==
                PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username or password.");

                return View(model);
            }

            var claims =
                new List<Claim>
                {
                    // Stores the user's ID
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.UserId.ToString()),

                    // Stores the username
                    new Claim(
                        ClaimTypes.Name,
                        user.Username),

                    // Stores the user's role
                    new Claim(
                        ClaimTypes.Role,
                        user.IsAdmin
                            ? "Admin"
                            : "User")
                };

            var claimsIdentity =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);

            var claimsPrincipal =
                new ClaimsPrincipal(
                    claimsIdentity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                claimsPrincipal);

            // Returns the user to the page
            // they came from
            if (!string.IsNullOrEmpty(
                    model.ReturnUrl)
                && Url.IsLocalUrl(
                    model.ReturnUrl))
            {
                return LocalRedirect(
                    model.ReturnUrl);
            }

            // Normal login from the navbar
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
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            TempData["SuccessMessage"] =
                "You have been logged out successfully.";

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}