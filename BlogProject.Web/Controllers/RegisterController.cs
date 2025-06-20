﻿using BlogProject.Core.Models.ViewModels;
using BlogProject.Data.Entities;
using BlogProject.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                Log.Error("RegisterController: Не удалось создать пользователя {Email}. Код ошибки: {Code}, Описание: {Description}", user.Email, error.Code, error.Description);
            }
            return View("Index", model);
        }

        // Назначаем роль
        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            Log.Error("RegisterController: Не удалось назначить роль '{Role}' пользователю {Email}. Ошибки: {@Errors}", role, user.Email, roleResult.Errors);
            throw new InvalidOperationException($"Ошибка назначения роли {role}");
        }

        Log.Information("RegisterController: Пользователь {Email} создан с ролью {Role}", user.Email, role);

        // Создаем claims для пользователя
        var principal = await claimsService.SaveNewClaimAsync(user);

        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });

        Log.Information("RegisterController:Пользователь {Email} зарегистрирован и авторизован", user.Email);

        return RedirectToAction("MainPage", "AccountManager");
    }
}