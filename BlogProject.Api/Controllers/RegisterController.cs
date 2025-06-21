using BlogProject.Api.Services;
using BlogProject.Core.Models.RequestModels;
using BlogProject.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BlogProject.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class RegisterController(UserManager<User> userManager, JwtService jwtService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };
        var role = "User";
        var password = request.NewPassword!;

        // Создаем пользователя
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                Log.Error("RegisterController: Не удалось создать пользователя {Email}. Код ошибки: {Code}, Описание: {Description}", user.Email, error.Code, error.Description);
            }
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        // Назначаем роль
        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            Log.Error("RegisterController: Не удалось назначить роль '{Role}' пользователю {Email}. Ошибки: {@Errors}", role, user.Email, roleResult.Errors);
            throw new InvalidOperationException($"Ошибка назначения роли {role}");
        }

        Log.Information("RegisterController: Пользователь {Email} создан с ролью {Role}", user.Email, role);

        return Ok(new { Message = $"Пользователь {user.Email} создан с ролью {role}", Message2 = "Пройдите авторизацию" });
    }
}