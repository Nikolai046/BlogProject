using BlogProject.Core.Models.ViewModels;
using BlogProject.Data.Entities;
using BlogProject.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace BlogProject.Web.Controllers;

[AllowAnonymous]
[Route("[controller]")]
public class LoginController(UserManager<User> userManager, UserClaimsService claimsService) : Controller
{
    public Task<IActionResult> Index()
    {
        return Task.FromResult<IActionResult>(View());
    }

    [ValidateAntiForgeryToken]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь не найден");
                return View("Index", model);
            }

            // Проверка пароля без входа в систему
            var isValidPassword = await userManager.CheckPasswordAsync(user, model.Password!);
            if (!isValidPassword)
            {
                ModelState.AddModelError(string.Empty, "Неверный пароль");
                return View("Index", model);
            }

            // Создаем claims principal
            var principal = await claimsService.CreateUserPrincipalAsync(user);

            //// Выполняем вход
            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe // RememberMe функционал
                });

            Log.Information("LoginController: Пользователь {Email} успешно вошел в систему", model.Email);
        }
        catch (Exception e)
        {
            Log.Error(e, "LoginController: Ошибка при входе пользователя {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Произошла ошибка при входе. Пожалуйста, попробуйте позже.");
            return View("Index", model);
        }

        return RedirectToAction("MainPage", "AccountManager");
    }

    [ValidateAntiForgeryToken]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        Log.Information("LoginController: Пользователь {Email} вышел из системы", userEmail);
        return RedirectToAction("Index", "Home");
    }
}