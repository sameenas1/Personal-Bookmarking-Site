using Microsoft.AspNetCore.Mvc;

namespace BookmarkSite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
