using Microsoft.AspNetCore.Mvc;

namespace GamificationPlatform.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

