using BlogProject.Core.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Web.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index(int statusCode, string requestId, string message)
    {
        var errorViewModel = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = statusCode,
            Message = message ?? "Произошла непредвиденная ошибка." // Сообщение по умолчанию
        };

        return View("Error", errorViewModel);
    }
}