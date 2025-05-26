using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using BlogProject.Core.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace BlogProject.Web.Controllers;

[AllowAnonymous] // Чтобы страница ошибки была доступна всем
public class ErrorController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index(int statusCode = 500, string? message = null)
    {
        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode,
            Message = message ?? "Произошла непредвиденная ошибка." // Сообщение по умолчанию
        };

        return View("Error", errorViewModel);
    }
}