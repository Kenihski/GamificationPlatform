using System.Security.Claims;
using GamificationPlatform.DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GamificationPlatform.Authentication
{
    // Rejects authentication cookies whose user no longer exists.
    // This can happen after the development database is recreated or
    // when an account is deleted while its browser cookie is still valid.
    public sealed class ValidateUserCookieEvents : CookieAuthenticationEvents
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ValidateUserCookieEvents> _logger;

        public ValidateUserCookieEvents(
            IUserRepository userRepository,
            ILogger<ValidateUserCookieEvents> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public override async Task ValidatePrincipal(
            CookieValidatePrincipalContext context)
        {
            string? userIdClaim =
                context.Principal?
                    .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning(
                    "Authentication cookie rejected because it did not contain a valid user identifier.");

                await RejectPrincipalAsync(context);
                return;
            }

            var user =
                await _userRepository
                    .GetUserByIdAsync(userId);

            if (user != null)
            {
                return;
            }

            _logger.LogWarning(
                "Authentication cookie rejected because user {UserId} no longer exists.",
                userId);

            await RejectPrincipalAsync(context);
        }

        // Preserve the shared 403 page behavior configured before this
        // custom events class was introduced.
        public override Task RedirectToAccessDenied(
            RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return Task.CompletedTask;
        }

        private static async Task RejectPrincipalAsync(
            CookieValidatePrincipalContext context)
        {
            context.RejectPrincipal();

            // Expire the invalid cookie in the browser immediately.
            await context.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
