using Microsoft.AspNetCore.Mvc;
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

        public UserController(
            IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
            if (ModelState.IsValid)
            {
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

                await _userRepository
                    .CreateUserAsync(user);

                // Sends the ReturnUrl to the login page.
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
                return View(model);
            }

            var user =
                await _userRepository
                    .GetUserByUsernameAsync(
                        model.Username);

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