
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GamificationPlatform.Models;
using GamificationPlatform.ViewModels;

namespace GamificationPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // Shows the registration form.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Creates a new user.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            string? returnUrl,
            bool isModal = false)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return RegistrationError(
                    model,
                    isModal,
                    "Please check the registration form.");
            }

            model.Username = model.Username.Trim();
            model.Email = model.Email.Trim();

            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "This username is already taken.");

                return RegistrationError(
                    model,
                    isModal,
                    "This username is already taken.");
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered.");

                return RegistrationError(
                    model,
                    isModal,
                    "This email is already registered.");
            }

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email
            };

            // Identity hashes the password and saves the user.
            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return RegistrationError(
                    model,
                    isModal,
                    "Registration failed.");
            }

            _logger.LogInformation(
                "User {UserId} registered successfully.",
                user.Id);

            // The popup will open the Login modal.
            if (isModal)
            {
                return Json(new
                {
                    success = true
                });
            }

            // Registration does not automatically log in.
            return RedirectToAction(
                nameof(Login),
                new
                {
                    returnUrl
                });
        }

        // Shows the login form.
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        // Logs the user in.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            bool isModal = false)
        {
            if (!ModelState.IsValid)
            {
                return LoginError(
                    model,
                    isModal,
                    "Please enter a valid username or email and password.");
            }

            model.Username = model.Username.Trim();

            // Accept either username or email.
            User? user;

            if (model.Username.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(
                    model.Username);

                // Also allow usernames containing '@'.
                user ??= await _userManager.FindByNameAsync(
                    model.Username);
            }
            else
            {
                user = await _userManager.FindByNameAsync(
                    model.Username);
            }

            if (user == null)
            {
                return LoginError(
                    model,
                    isModal,
                    "Invalid username/email or password.");
            }

            // Identity verifies the password and creates the cookie.
            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                var message = result.IsLockedOut
                    ? "Your account is temporarily locked. Please try again later."
                    : "Invalid username/email or password.";

                return LoginError(
                    model,
                    isModal,
                    message);
            }

            _logger.LogInformation(
                "User {UserId} logged in successfully.",
                user.Id);

            // The popup will refresh the current page.
            if (isModal)
            {
                return Json(new
                {
                    success = true
                });
            }

            // Return only to local URLs.
            if (!string.IsNullOrEmpty(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction(
                "Grid",
                "Challenge");
        }

        // Logs the user out.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = _userManager.GetUserId(User);

            await _signInManager.SignOutAsync();

            _logger.LogInformation(
                "User {UserId} logged out successfully.",
                userId);

            TempData["SuccessMessage"] =
                "You have been logged out successfully.";

            return RedirectToAction(
                "Index",
                "Home");
        }

        // Shows an access denied page.
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Handles registration errors for normal and modal forms.
        private IActionResult RegistrationError(
            RegisterViewModel model,
            bool isModal,
            string fallbackMessage)
        {
            _logger.LogWarning("User registration failed.");

            if (isModal)
            {
                var errors = ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry =>
                        entry.Value!.Errors.Select(error =>
                            error.ErrorMessage))
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = errors.Count > 0
                        ? string.Join(" ", errors)
                        : fallbackMessage
                });
            }

            return View("Register", model);
        }

        // Handles login errors for normal and modal forms.
        private IActionResult LoginError(
            LoginViewModel model,
            bool isModal,
            string message)
        {
            _logger.LogWarning("User login failed.");

            if (isModal)
            {
                return Json(new
                {
                    success = false,
                    message
                });
            }

            ModelState.AddModelError(
                string.Empty,
                message);

            return View("Login", model);
        }
    }
}
