using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

using System.Security.Claims;

namespace GamificationPlatform.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }


        // Shows all users - admin only.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Table()
        {
            List<User> users =
                await _userRepository
                    .GetAllUsersAsync();

            return View(users);
        }


        // Shows details and history for one user - admin only.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var user =
                await _userRepository
                    .GetUserWithDetailsAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Calculates max points for challenges
            // created by this user.
            foreach (var challenge in user.CreatedChallenges)
            {
                challenge.MaxPoints =
                    challenge.Questions
                        .Sum(q => q.Points);
            }

            var viewModel =
                new UserDetailsViewModel
                {
                    User = user,

                    CreatedChallenges =
                        user.CreatedChallenges
                };

            // Prepares completed challenge history
            // for the view.
            foreach (var userChallenge in
                user.UserChallenges)
            {
                var attempts =
                    userChallenge.Attempts
                        .Where(a => a.Completed)
                        .OrderByDescending(a =>
                            a.CompletedAt)
                        .ToList();

                if (!attempts.Any())
                {
                    continue;
                }

                var chronologicalAttempts =
                    attempts
                        .OrderBy(a => a.CompletedAt)
                        .ToList();

                var latestAttempt =
                    attempts.First();

                var bestAttempt =
                    attempts
                        .OrderByDescending(a =>
                            a.Score)
                        .ThenBy(a =>
                            (a.CompletedAt ??
                             a.StartedAt)
                            - a.StartedAt)
                        .First();

                int maxPoints =
                    userChallenge.Challenge
                        .Questions
                        .Sum(q => q.Points);

                var challengeHistory =
                    new UserChallengeHistoryViewModel
                    {
                        ChallengeTitle =
                            userChallenge.Challenge.Title,

                        MaxPoints =
                            maxPoints
                    };

                foreach (var attempt in attempts)
                {
                    int attemptNumber =
                        chronologicalAttempts
                            .FindIndex(a =>
                                a.ChallengeAttemptId ==
                                attempt.ChallengeAttemptId)
                        + 1;

                    string? timeUsed = null;

                    if (attempt.CompletedAt.HasValue)
                    {
                        var duration =
                            attempt.CompletedAt.Value -
                            attempt.StartedAt;

                        if (duration.TotalMinutes >= 1)
                        {
                            timeUsed =
                                $"{Math.Round(duration.TotalMinutes)} min";
                        }
                        else
                        {
                            timeUsed =
                                $"{Math.Round(duration.TotalSeconds)} sec";
                        }
                    }

                    challengeHistory.Attempts.Add(
                        new UserAttemptHistoryViewModel
                        {
                            ChallengeAttemptId =
                                attempt.ChallengeAttemptId,

                            AttemptNumber =
                                attemptNumber,

                            Score =
                                attempt.Score,

                            TimeUsed =
                                timeUsed,

                            CompletedAt =
                                attempt.CompletedAt?
                                    .ToString(
                                        "dd.MM.yyyy HH:mm"),

                            IsBest =
                                attempt.ChallengeAttemptId ==
                                bestAttempt
                                    .ChallengeAttemptId,

                            IsLatest =
                                attempt.ChallengeAttemptId ==
                                latestAttempt
                                    .ChallengeAttemptId
                        });
                }

                viewModel.ChallengeHistory.Add(
                    challengeHistory);
            }

            return View(viewModel);
        }


        // Shows the registration form.
        [HttpGet]
        public IActionResult Register(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }


        // Creates a new user.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                model.Username = model.Username.Trim();
                model.Email = model.Email.Trim();

                if (await _userRepository.GetUserByUsernameAsync(model.Username) != null)
                {
                    ModelState.AddModelError(nameof(model.Username), "This username is already taken.");
                    _logger.LogWarning("Registration rejected because the username is already taken.");
                    return View(model);
                }

                var user =
                    new User
                    {
                        Username = model.Username,
                        Email = model.Email,

                        // New users are normal users by default.
                        IsAdmin = false
                    };

                var passwordHasher =
                    new PasswordHasher<User>();

                user.PasswordHash =
                    passwordHasher.HashPassword(
                        user,
                        model.Password);

                try
                {
                    await _userRepository.CreateUserAsync(user);
                }
                catch (DbUpdateException ex) when (
                    ex.InnerException is SqliteException sqlite &&
                    sqlite.SqliteExtendedErrorCode == 2067 &&
                    sqlite.Message.Contains("Users.Username", StringComparison.Ordinal))
                {
                    // A competing registration may win after the initial check.
                    ModelState.AddModelError(nameof(model.Username), "This username is already taken.");
                    _logger.LogWarning("Registration rejected by the unique username constraint.");
                    return View(model);
                }

                _logger.LogInformation("User {UserId} registered successfully.", user.UserId);

                // Sends the ReturnUrl to the login page.
                return RedirectToAction(
                    nameof(Login),
                    new
                    {
                        returnUrl = returnUrl
                    });
            }

            _logger.LogWarning("Registration rejected because form validation failed.");
            return View(model);
        }


        // Shows the login form.
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


        // Logs the user in.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login rejected because form validation failed.");
                return View(model);
            }

            model.Username = model.Username.Trim();

            var user =
                await _userRepository
                    .GetUserByUsernameAsync(
                        model.Username);

            if (user == null)
            {
                _logger.LogWarning("Login failed because the credentials were invalid.");
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
                _logger.LogWarning("Login failed because the credentials were invalid.");
                ModelState.AddModelError(
                    "",
                    "Invalid username or password.");

                return View(model);
            }

            var claims =
                new List<Claim>
                {
                    // Stores the user's ID.
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.UserId.ToString()),

                    // Stores the username.
                    new Claim(
                        ClaimTypes.Name,
                        user.Username),

                    // Stores the user's role.
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

            _logger.LogInformation("User {UserId} logged in successfully.", user.UserId);

            // Returns the user to the page
            // they came from.
            if (!string.IsNullOrEmpty(
                    model.ReturnUrl)
                && Url.IsLocalUrl(
                    model.ReturnUrl))
            {
                return LocalRedirect(
                    model.ReturnUrl);
            }

            // Normal login from the navbar.
            return RedirectToAction(
                "Grid",
                "Challenge");
        }


        // Logs the user out.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            _logger.LogInformation("User {UserId} logged out successfully.", userId);

            TempData["SuccessMessage"] =
                "You have been logged out successfully.";

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}
