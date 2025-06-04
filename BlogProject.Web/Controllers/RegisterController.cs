using BlogProject.Core.Models.ViewModels;
using BlogProject.Data.Entities;
using BlogProject.Data.Seeder;
using BlogProject.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using Serilog;

namespace BlogProject.Web.Controllers;

public class RegisterController(UserManager<User> userManager, UserClaimsService claimsService) : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Ошибка создания пользователя");
            return View("Index", model);
        }

        var user = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            UserName = model.Email
        };
        var role = "User";
        var password = model.NewPassword!;

        // Создаем пользователя
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            Log.Error("Ошибка создания пользователя", result.Errors);
            throw new InvalidOperationException("Ошибка создания пользователя");
        }

        // Назначаем роль
        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            Log.Error($"Ошибка назначения роли {role}", roleResult.Errors);
            throw new InvalidOperationException($"Ошибка назначения роли {role}");
        }

        Log.Information("Пользователь {FirstName} {LastName} создан с ролью {Role}",
            user.FirstName, user.LastName, role);

        // Создаем claims для пользователя
        var principal = await claimsService.SaveNewClaimAsync(user);

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });

        Log.Information("Пользователь {FirstName} {LastName} зарегистрирован и авторизован", user.FirstName, user.LastName);

        return RedirectToAction("MainPage", "AccountManager");
    }
}