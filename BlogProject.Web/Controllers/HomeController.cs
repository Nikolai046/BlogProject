using BlogProject.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BlogProject.Core.Models.ViewModels;

namespace BlogProject.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
