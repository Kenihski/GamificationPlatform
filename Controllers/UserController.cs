using Microsoft.AspNetCore.Mvc;
using GamificationPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace GamificationPlatform.Controllers
{
    public class UserController : Controller
    {
        private readonly ChallengeDbContext _challengeDbContext;

        public UserController(ChallengeDbContext challengeDbContext)
        {
            _challengeDbContext = challengeDbContext;
        }

        public async Task<IActionResult> Table()
        {
            List<User> users =
                await _challengeDbContext.Users.ToListAsync();

            return View(users);
        }
    }
}
