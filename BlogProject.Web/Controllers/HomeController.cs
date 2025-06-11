using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Web.Controllers
{
    public class HomeController() : Controller
    {
        public Task<IActionResult> Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return Task.FromResult<IActionResult>(RedirectToAction("MainPage", "AccountManager"));
            }
            else
            {
                // Редирект на страницу логина
                return Task.FromResult<IActionResult>(RedirectToAction("Index", "Login"));
            }
        }
    }
}